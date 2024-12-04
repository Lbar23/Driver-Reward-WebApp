using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;
using Serilog;

namespace Backend_Server.Services
{
    public interface IJwtKeyProvider
    {
        Task<byte[]> GetJwtKeyAsync();
    }

    public class AwsJwtKeyProvider(IAmazonSecretsManager secretsManager) : IJwtKeyProvider
    {
        private readonly IAmazonSecretsManager _secretsManager = secretsManager;
        private byte[]? _cachedKey;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public async Task<byte[]> GetJwtKeyAsync()
        {
            if (_cachedKey != null)
            {
                return _cachedKey;
            }

            try
            {
                await _semaphore.WaitAsync();
                
                if (_cachedKey != null) // Double check after acquiring lock
                {
                    return _cachedKey;
                }

                var secretRequest = new GetSecretValueRequest
                {
                    SecretId = "team16/ec2-instance/ssh-credentials"
                };

                var secretResponse = await _secretsManager.GetSecretValueAsync(secretRequest);
                var secretJson = JsonSerializer.Deserialize<Dictionary<string, string>>(secretResponse.SecretString)
                                 ?? throw new Exception("Failed to load JWT Key");
                
                _cachedKey = Convert.FromBase64String(secretJson["jwt-secret-key"]);
                return _cachedKey;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve JWT key from AWS Secrets Manager");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}