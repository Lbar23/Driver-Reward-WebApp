using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Renci.SshNet;
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
        private SshClient? _sshClient;
        private ForwardedPortLocal? _forwardedPort;
        private string? _tempKeyPath;

        public DbConnectionProvider(IConfiguration configuration, IAmazonSecretsManager secretsManager)
        {
            _configuration = configuration;
            _secretsManager = secretsManager;
        }

        public async Task<MySqlConnection> GetDbConnectionAsync()
        {
            var (dbSecrets, sshSecrets) = await GetSecrets();
            await SetupSshTunnel(dbSecrets, sshSecrets);

            string connectionString = (_configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string not found."))  //added null check
                .Replace("{Username}", dbSecrets["username"].GetString())
                .Replace("{Password}", dbSecrets["password"].GetString())
                .Replace("{Host}", "127.0.0.1")
                .Replace("{Port}", _forwardedPort?.BoundPort.ToString());

            return new MySqlConnection(connectionString);
        }

        private async Task<(Dictionary<string, JsonElement> dbSecrets, Dictionary<string, string> sshSecrets)> GetSecrets()
        {
            var dbSecretStr = await GetSecret("team16/rds-instance/db-credentials");
            var dbSecrets = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dbSecretStr)
                ?? throw new InvalidOperationException("Failed to deserialize SSH secrets");
            var sshSecretStr = await GetSecret("team16/ec2-instance/ssh-credentials");
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
            
            // Remove any existing formatting
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

        public void Dispose()
        {
            _forwardedPort?.Stop();
            _sshClient?.Disconnect();
            _sshClient?.Dispose();
            if (File.Exists(_tempKeyPath))
            {
                File.Delete(_tempKeyPath);
            }
        }
    }
}