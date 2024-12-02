using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Renci.SshNet;
using Serilog;

namespace Backend_Server.Infrastructure
{
    public class DbConnectionProvider : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IAmazonSecretsManager _secretsManager;

        private static readonly SemaphoreSlim _tunnelLock = new(1, 1);
        private static volatile SshClient? _sshClient;
        private static volatile ForwardedPortLocal? _forwardedPort;
        private static volatile string? _tempKeyPath;
        private static volatile string? _cachedConnectionString;
        private static volatile bool _isInitialized;
        private static readonly object _initLock = new();
        private bool _disposed;

        public DbConnectionProvider(IConfiguration configuration, IAmazonSecretsManager secretsManager)
        {
            _configuration = configuration;
            _secretsManager = secretsManager;
        }

        public async Task<MySqlConnection> GetDbConnectionAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DbConnectionProvider));

            await EnsureSshTunnelAsync();

            // Use cached connection string if available
            if (!string.IsNullOrEmpty(_cachedConnectionString))
                return new MySqlConnection(_cachedConnectionString);

            var (dbSecrets, _) = await GetSecrets();
            _cachedConnectionString = BuildConnectionString(dbSecrets);

            return new MySqlConnection(_cachedConnectionString);
        }

        private async Task EnsureSshTunnelAsync()
        {
            if (_isInitialized && _sshClient?.IsConnected == true)
                return;

            await _tunnelLock.WaitAsync();
            try
            {
                if (_isInitialized && _sshClient?.IsConnected == true)
                    return;

                Log.Information("Setting up SSH tunnel");
                await SetupTunnelAsync();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to set up SSH tunnel");
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

        private string BuildConnectionString(Dictionary<string, JsonElement> dbSecrets)
        {
            var baseString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string not found.");

            return baseString
                .Replace("{Username}", dbSecrets["username"].GetString())
                .Replace("{Password}", dbSecrets["password"].GetString())
                .Replace("{Host}", "127.0.0.1")
                .Replace("{Port}", _forwardedPort?.BoundPort.ToString())
                + "Allow User Variables=True;SslMode=None;";
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
                sb.AppendLine(i + LINE_LENGTH >= key.Length
                    ? key[i..]
                    : key.Substring(i, LINE_LENGTH));
            }

            sb.AppendLine("-----END RSA PRIVATE KEY-----");
            return sb.ToString();
        }

        private void CleanupTunnel()
        {
            try
            {
                _forwardedPort?.Stop();
                _forwardedPort?.Dispose();
                _forwardedPort = null;

                _sshClient?.Disconnect();
                _sshClient?.Dispose();
                _sshClient = null;

                if (!string.IsNullOrEmpty(_tempKeyPath) && File.Exists(_tempKeyPath))
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
