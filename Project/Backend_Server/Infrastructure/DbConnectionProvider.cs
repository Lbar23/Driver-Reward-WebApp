using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using MySql.Data.MySqlClient;
using Renci.SshNet;
using Serilog;
using System.IO;
using System.Collections.Concurrent;
using System.Net.Sockets;

/// <summary>
/// Provider Infrastructure Class for database connection in multiple instances, since the same connection must be the same for any usage of it.
/// Only denoted for really Program.cs or Service classes that themselves need to connect to the database before Program.cs starts running
/// Rarely used for controllers; AppDBContext handles that aspect of API <-> Database connection
/// </summary>
namespace Backend_Server.Infrastructure
{
    public class DbConnectionProvider : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IAmazonSecretsManager _secretsManager;
        
        // Static fields for sharing across instances
        private static readonly SemaphoreSlim _tunnelLock = new(1, 1);
        private static volatile SshClient? _sshClient;
        private static volatile ForwardedPortLocal? _forwardedPort;
        private static volatile string? _tempKeyPath;
        private static volatile string? _cachedConnectionString;
        private static volatile bool _isInitialized;
        private static readonly object _initLock = new();
        private bool _disposed;

        //Added Connection Resilience instead of instantly disposing failed connections
        private const int MAX_RECONNECT_ATTEMPTS = 3;
        private const int RECONNECT_DELAY_MS = 1000;
        private static readonly TimeSpan KEEP_ALIVE_INTERVAL = TimeSpan.FromSeconds(30);
        private static System.Timers.Timer? _keepAliveTimer;
        private static readonly ConcurrentDictionary<string, DateTime> _lastActivityTime = new();
        private static volatile bool _isReconnecting;

        public DbConnectionProvider(
            IConfiguration configuration,
            IAmazonSecretsManager secretsManager)
        {
            _configuration = configuration;
            _secretsManager = secretsManager;
            InitializeKeepAlive();
        }

        private void InitializeKeepAlive()
        {
            if (_keepAliveTimer == null)
            {
                lock (_initLock)
                {
                    if (_keepAliveTimer == null)
                    {
                        _keepAliveTimer = new System.Timers.Timer(KEEP_ALIVE_INTERVAL.TotalMilliseconds);
                        _keepAliveTimer.Elapsed += async (s, e) => await CheckConnectionHealth();
                        _keepAliveTimer.Start();
                    }
                }
            }
        }

        private async Task CheckConnectionHealth()
        {
            if (_isReconnecting || !_isInitialized) return;

            try
            {
                if (_sshClient?.IsConnected == true)
                {
                    // Test SSH connection
                    _sshClient.CreateCommand("echo 1").Execute();

                    // Test database connection
                    if (_cachedConnectionString != null)
                    {
                        using var conn = new MySqlConnection(_cachedConnectionString);
                        await conn.OpenAsync();
                        using var cmd = conn.CreateCommand();
                        cmd.CommandText = "SELECT 1";
                        await cmd.ExecuteScalarAsync();
                    }
                }
                else
                {
                    await AttemptReconnection();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Connection health check failed, attempting reconnection...");
                await AttemptReconnection();
            }
        }

        private async Task AttemptReconnection()
        {
            if (_isReconnecting) return;

            _isReconnecting = true;
            try
            {
                for (int attempt = 0; attempt < MAX_RECONNECT_ATTEMPTS; attempt++)
                {
                    try
                    {
                        CleanupTunnel();
                        await SetupTunnelAsync();
                        Log.Information("Successfully reconnected on attempt {Attempt}", attempt + 1);
                        return;
                    }
                    catch (Exception ex) when (attempt < MAX_RECONNECT_ATTEMPTS - 1)
                    {
                        Log.Warning(ex, "Reconnection attempt {Attempt} failed", attempt + 1);
                        await Task.Delay(RECONNECT_DELAY_MS * (attempt + 1));
                    }
                }
            }
            finally
            {
                _isReconnecting = false;
            }
        }

        public async Task<MySqlConnection> GetDbConnectionAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DbConnectionProvider));
            }

            for (int attempt = 0; attempt < MAX_RECONNECT_ATTEMPTS; attempt++)
            {
                try
                {
                    await EnsureSshTunnelAsync();

                    if (_cachedConnectionString == null)
                    {
                        var (dbSecrets, _) = await GetSecrets();
                        _cachedConnectionString = BuildConnectionString(dbSecrets);
                    }

                    var connection = new MySqlConnection(_cachedConnectionString);
                    await connection.OpenAsync();
                    
                    // Test the connection
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT 1";
                        await cmd.ExecuteScalarAsync();
                    }

                    UpdateActivityTime();
                    return connection;
                }
                catch (Exception ex) when (attempt < MAX_RECONNECT_ATTEMPTS - 1 && 
                    (ex is MySqlException || ex is SocketException))
                {
                    Log.Warning(ex, "Connection attempt {Attempt} failed, retrying...", attempt + 1);
                    await Task.Delay(RECONNECT_DELAY_MS * (attempt + 1));
                    
                    // Force tunnel recreation on failure
                    if (attempt == 0)
                    {
                        await AttemptReconnection();
                    }
                }
            }

            throw new InvalidOperationException("Failed to establish database connection after retries");
        }

        private void UpdateActivityTime()
        {
            var sessionId = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            _lastActivityTime.AddOrUpdate(sessionId, DateTime.UtcNow, (_, __) => DateTime.UtcNow);
        }

        private async Task EnsureSshTunnelAsync()
        {
            // Quick check without locking
            if (_isInitialized && _sshClient?.IsConnected == true)
            {
                return;
            }

            await _tunnelLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_isInitialized && _sshClient?.IsConnected == true)
                {
                    return;
                }

                if (_sshClient != null && _sshClient.IsConnected)
                {
                    Log.Warning("Already established connection...");
                    return;
                }
                
                Log.Information("Setting up new SSH tunnel");
                await SetupTunnelAsync();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to setup SSH tunnel");
                throw;
            }
            finally
            {
                _tunnelLock.Release();
            }
        }

        private async Task SetupTunnelAsync()
        {
            var (dbSecrets, sshSecrets) = await GetSecrets();

            CleanupTunnel();

            _tempKeyPath = await CreateTempSshKey(sshSecrets["keypath"]);
            _sshClient = new SshClient(sshSecrets["host"], sshSecrets["username"], new PrivateKeyFile(_tempKeyPath));

            try
            {
                _sshClient.Connect();
                Log.Information("SSH client connected successfully");

                _forwardedPort = new ForwardedPortLocal("127.0.0.1",
                    dbSecrets["port"].GetUInt32(),
                    dbSecrets["host"].GetString(),
                    dbSecrets["port"].GetUInt32());

                _sshClient.AddForwardedPort(_forwardedPort);
                _forwardedPort.Start();

                Log.Information("SSH tunnel established on port {Port}", _forwardedPort.BoundPort);
            }
            catch
            {
                CleanupTunnel();
                throw;
            }
        }

        private string BuildConnectionString(Dictionary<string, JsonElement> dbSecrets)
        {
            var baseString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string not found.");
            
            //Check to ensure password carries to the connection string from secrets manager
            var password = dbSecrets["password"].GetString();
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Database password not found in secrets");
            }

            return baseString
                .Replace("{Username}", dbSecrets["username"].GetString())
                .Replace("{Password}", dbSecrets["password"].GetString())
                .Replace("{Host}", "127.0.0.1")
                .Replace("{Port}", _forwardedPort?.BoundPort.ToString())
                + ";Convert Zero Datetime=True;Allow Zero Datetime=True;"
                + "Pooling=true;Min Pool Size=5;Max Pool Size=100;"
                + "Connection Lifetime=300;Connection Timeout=30;"
                + "Keep Alive=30;Allow User Variables=true;"
                + "Connect Timeout=30;Default Command Timeout=30;"
                + "Auto Enlist=true;Persist Security Info=true"; //Extra requirements for DB connection to remain efficient and secure (scalable as well)
        }

        private async Task<(Dictionary<string, JsonElement> dbSecrets, Dictionary<string, string> sshSecrets)> GetSecrets()
        {
            var dbSecretStr = await GetSecret("team16/rds-instance/db-credentials");
            var sshSecretStr = await GetSecret("team16/ec2-instance/ssh-credentials");

            var dbSecrets = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dbSecretStr)
                ?? throw new InvalidOperationException("Failed to deserialize DB secrets");
            var sshSecrets = JsonSerializer.Deserialize<Dictionary<string, string>>(sshSecretStr)
                ?? throw new InvalidOperationException("Failed to deserialize SSH secrets");

            return (dbSecrets, sshSecrets);
        }

        private async Task<string> GetSecret(string secretName)
        {
            var response = await _secretsManager.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            });
            return response.SecretString;
        }

        private async Task SetupSshTunnel(Dictionary<string, JsonElement> dbSecrets, Dictionary<string, string> sshSecrets)
        {
            Dispose();
            _tempKeyPath = await CreateTempSshKey(sshSecrets["keypath"]);
            _sshClient = new SshClient(sshSecrets["host"], sshSecrets["username"], new PrivateKeyFile(_tempKeyPath));
            _sshClient.Connect();

            _forwardedPort = new ForwardedPortLocal("127.0.0.1", 
                dbSecrets["port"].GetUInt32(), 
                dbSecrets["host"].GetString(), 
                dbSecrets["port"].GetUInt32());
            _sshClient.AddForwardedPort(_forwardedPort);
            _forwardedPort.Start();
        }

        private static async Task<string> CreateTempSshKey(string keyContent)
        {
            var tempKeyPath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempKeyPath, FormatSshKey(keyContent));
            return tempKeyPath;
        }

        private static string FormatSshKey(string key)
        {
            const int LINE_LENGTH = 64;
            var sb = new StringBuilder();
            sb.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
            
            key = key.Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                    .Replace("-----END RSA PRIVATE KEY-----", "")
                    .Replace("\n", "")
                    .Replace("\r", "");
            
            for (int i = 0; i < key.Length; i += LINE_LENGTH)
            {
                if (i + LINE_LENGTH >= key.Length)
                    sb.AppendLine(key.Substring(i));
                else
                    sb.AppendLine(key.Substring(i, LINE_LENGTH));
            }
            
            sb.AppendLine("-----END RSA PRIVATE KEY-----");
            return sb.ToString();
        }

        private void CleanupTunnel()
        {
            try
            {
                if (_forwardedPort != null)
                {
                    if (_forwardedPort.IsStarted)
                    {
                        _forwardedPort.Stop();
                    }
                    _forwardedPort.Dispose();
                    _forwardedPort = null;
                }

                if (_sshClient != null)
                {
                    if (_sshClient.IsConnected)
                    {
                        _sshClient.Disconnect();
                    }
                    _sshClient.Dispose();
                    _sshClient = null;
                }

                if (_tempKeyPath != null && File.Exists(_tempKeyPath))
                {
                    File.Delete(_tempKeyPath);
                    _tempKeyPath = null;
                }

                _isInitialized = false;
                _cachedConnectionString = null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during tunnel cleanup");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_initLock)
            {
                if (_disposed) return;
                
                _keepAliveTimer?.Stop();
                _keepAliveTimer?.Dispose();
                _keepAliveTimer = null;
                
                CleanupTunnel();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}