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

        public DbConnectionProvider(
            IConfiguration configuration,
            IAmazonSecretsManager secretsManager)
        {
            _configuration = configuration;
            _secretsManager = secretsManager;
        }

        public async Task<MySqlConnection> GetDbConnectionAsync()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DbConnectionProvider));
            }

            await EnsureSshTunnelAsync();

            // Use cached connection string if available
            if (_cachedConnectionString != null)
            {
                return new MySqlConnection(_cachedConnectionString);
            }

            var (dbSecrets, _) = await GetSecrets();
            _cachedConnectionString = BuildConnectionString(dbSecrets);

            return new MySqlConnection(_cachedConnectionString);
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

            return baseString
                .Replace("{Username}", dbSecrets["username"].GetString())
                .Replace("{Password}", dbSecrets["password"].GetString())
                .Replace("{Host}", "127.0.0.1")
                .Replace("{Port}", _forwardedPort?.BoundPort.ToString())
                + ";Convert Zero Datetime=True;Allow Zero Datetime=True;"
                + "Pooling=true;Min Pool Size=5;Max Pool Size=100;"
                + "Connection Lifetime=300;Connection Timeout=30;"; //Extra requirements for DB connection to remain efficient and secure (scalable as well)
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
                CleanupTunnel();
                _disposed = true;
            }

            GC.SuppressFinalize(this);
        }
    }
}