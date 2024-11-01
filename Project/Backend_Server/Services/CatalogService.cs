using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace Backend_Server.Services
{
    public class CatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatalogService> _logger;

        private readonly string _tokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private const string BrowseUrl = "https://api.ebay.com/buy/browse/v1/item_summary/search?category_ids=6030&q=accessories&limit=20";        private static string _cachedToken;
        private static DateTime _tokenExpiration = DateTime.MinValue;

        public CatalogService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IAmazonSecretsManager secretsManager, ILogger<CatalogService> logger)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            var secrets = LoadSecrets(secretsManager).Result;
            _tokenUrl = secrets["EbayProdOauthUrl"];
            _clientId = secrets["EbayClientID"];
            _clientSecret = secrets["EbaySecret"];
        }

        private static async Task<Dictionary<string, string>> LoadSecrets(IAmazonSecretsManager secretsManager)
        {
            var secretValueResponse = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = "team16/catalog-api/creds"
            });

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(secretValueResponse.SecretString) ?? new Dictionary<string, string>();
        }

        public async Task<string> GetAppTokenAsync()
        {
            // Check if the cached token is still valid
            if (_cachedToken != null && DateTime.UtcNow < _tokenExpiration)
            {
                return _cachedToken;
            }

            // Request a new token if none is cached or the cached token has expired
            var authHeader = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("scope", "https://api.ebay.com/oauth/api_scope")
            });

            var response = await _httpClient.PostAsync(_tokenUrl, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            
            var tokenData = JsonConvert.DeserializeObject<TokenResponse>(result);

            // Cache the token and set the expiration time
            _cachedToken = tokenData.Access_token;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn - 300); // Refresh 5 minutes early for safety buffer

            return _cachedToken;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            var token = await GetAppTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(BrowseUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var productResponse = JsonConvert.DeserializeObject<EbayProductResponse>(content);
            // _logger.LogInformation("eBay API response: {ResponseContent}", content);


            return productResponse?.ItemSummaries?.ConvertAll(item => new Product
            {
                Name = item.Title.ToString(),
                ImageUrl = item.Image.ImageUrl,
                Price = $"{item.Price.Value} {item.Price.Currency}"
            }) ?? [];
        }

        // Model classes
        public class Product
        {
            public required string Name { get; set; }
            public required string ImageUrl { get; set; }
            public required string Price { get; set; }
        }

        public class EbayProductResponse
        {
            [JsonProperty("itemSummaries")]
            public required List<ItemSummary> ItemSummaries { get; set; }
        }

        public class TokenResponse
        {
            [JsonProperty("access_token")]
            public required string Access_token { get; set; }

            [JsonProperty("expires_in")]
            public required int ExpiresIn { get; set; } // Number of seconds until the token expires
        }

        public class ItemSummary
        {
            [JsonProperty("title")]
            public required string Title { get; set; }

            [JsonProperty("image")]
            public required Image Image { get; set; }

            [JsonProperty("price")]
            public required Price Price { get; set; }
        }

        public class Image
        {
            [JsonProperty("imageUrl")]
            public required string ImageUrl { get; set; }
        }

        public class Price
        {
            [JsonProperty("value")]
            public required string Value { get; set; }

            [JsonProperty("currency")]
            public required string Currency { get; set; }
        }
    }
}
