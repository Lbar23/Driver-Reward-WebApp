using System.Net.Http.Headers;
using Newtonsoft.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Backend_Server.Models;
using MySqlConnector;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend_Server.Services
{
    public class CatalogService{
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatalogService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private readonly string _tokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private const string BrowseUrl = "https://api.ebay.com/buy/browse/v1/item_summary/search?";        
        private static string? _cachedToken;
        private static DateTime _tokenExpiration = DateTime.MinValue;

        public enum EbayCategory
            {
                Accessories = 6030,
                Electronics = 293,
                Clothing = 11450,
                HomeAndGarden = 11700
            }


        public CatalogService(IHttpClientFactory httpClientFactory, 
                              IAmazonSecretsManager secretsManager, 
                              IServiceProvider serviceProvider, // Added for resolving scoped services
                              ILogger<CatalogService> logger)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _serviceProvider = serviceProvider; // Store the service provider
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
            _cachedToken = tokenData!.Access_token;
            _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.ExpiresIn - 300); // Refresh 5 minutes early for safety buffer

            return _cachedToken;
        }

        public async Task<List<Products>> GetProductsAsync(Sponsors sponsor)
        {
            var token = await GetAppTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(BrowseUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var productResponse = JsonConvert.DeserializeObject<EbayProductResponse>(content);
            _logger.LogInformation("eBay API response: {ResponseContent}", content);


            

            return productResponse?.ItemSummaries?.ConvertAll(item => new Products
            {
                SponsorID = sponsor.SponsorID,
                ProductName = item.Title.ToString(),
                PriceInPoints = (int)(item.Price.Value * sponsor.PointDollarValue),
                ImageUrl = item.Image.ImageUrl,
                CurrencyPrice = item.Price.Value,
                ExternalID = item.ItemId,
                Category = item.Categories.ToString(),
                Description = ParseDescription(item)
            }) ?? [];
        }

      private static string ParseDescription(ItemSummary item)
    {
        // Extract relevant fields from api response
        var title = item.Title;
        var condition = item.Condition ?? "Condition unspecified";
        var discount = string.Empty;
        var price = $"{item.Price.Value} {item.Price.Currency}";

        if (item?.MarketingPrice != null)
        {
            var discountValue = item.MarketingPrice.DiscountAmount?.Value ?? 0;
            var priceTreatment = item.MarketingPrice.PriceTreatment?.ToLower() ?? "special";
            var discountPercentage = item.MarketingPrice.DiscountPercentage;
            if (discountValue > 0)
            {
                discount = $"Now {priceTreatment}! Save {discountPercentage}%."; 
            }
        }

        // Take the first leaf category name, or the first category name if no leaf categories
        var categoryFocus = item.Categories
            .FirstOrDefault()?.CategoryName ?? "Miscellaneous";

        // Build the description
        return $"{title}: {condition}. Ideal for {categoryFocus}. Priced at {price}. {discount}".Trim();
    }

    public async Task<List<Products>> CreateCatalogAsync(int categoryId, int numberOfProducts, int userId)
    {
        var token = await GetAppTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();

        // Fetch sponsor details
        var sponsor = await context.SponsorUsers
            .Where(su => su.UserID == userId)
            .Include(su => su.Sponsor)
            .FirstOrDefaultAsync();

        if (sponsor?.Sponsor == null)
        {
            _logger.LogError("No sponsor found for user ID {UserId}", userId);
            throw new Exception($"No sponsor found for user ID {userId}");
        }

        try
        {
            // Make API call for the single category
            var apiUrl = $"{BrowseUrl}category_ids={categoryId}&limit={numberOfProducts}";
            _logger.LogInformation("Calling eBay API with URL: {ApiUrl}", apiUrl);
            
            var response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("eBay API response for category {CategoryId}: {Response}", categoryId, content);

            var productResponse = JsonConvert.DeserializeObject<EbayProductResponse>(content);

            if (productResponse?.ItemSummaries == null || !productResponse.ItemSummaries.Any())
            {
                _logger.LogWarning("No products retrieved for category {CategoryId}", categoryId);
                return new List<Products>();
            }

            var products = productResponse.ItemSummaries.Select(item => new Products
            {
                SponsorID = sponsor.Sponsor.SponsorID,
                // ProductID = Math.Abs(item.ItemId.GetHashCode()),
                ProductName = ShortenTitle(item.Title),
                Description = ParseDescription(item),
                Category = categoryId.ToString(),
                CurrencyPrice = item.Price.Value,
                PriceInPoints = (int)(item.Price.Value * sponsor.Sponsor.PointDollarValue),
                ExternalID = item.ItemId,
                ImageUrl = item.Image.ImageUrl,
                Availability = true
            }).ToList();

            // Save to database
            foreach (var product in products)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync(
                        "CALL sp_CreateSponsorCatalog(@SponsorID, @ProductID, @Name, @Description, @Category, @CurrencyPrice, @PriceInPoints, @ExternalID, @ImageUrl, @Availability)",
                        new MySqlParameter[] {
                            new("@SponsorID", product.SponsorID),
                            // new("@ProductID", product.ProductID),
                            new("@Name", product.ProductName),
                            new("@Description", product.Description),
                            new("@Category", product.Category),
                            new("@CurrencyPrice", product.CurrencyPrice),
                            new("@PriceInPoints", product.PriceInPoints),
                            new("@ExternalID", product.ExternalID),
                            new("@ImageUrl", product.ImageUrl),
                            new("@Availability", product.Availability)
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving product {ProductId} to database", product.ProductID);
                    // Continue with the next product rather than failing the entire batch
                }
            }

            _logger.LogInformation("Successfully processed {Count} products for category {CategoryId}", 
                products.Count, categoryId);
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing category {CategoryId}", categoryId);
            throw;
        }
    }






        public async Task<bool> CheckProductAvailabilityAsync(string itemId)
        {
            var token = await GetAppTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var productUrl = $"https://api.ebay.com/buy/browse/v1/item/{itemId}";

            try
            {
                var response = await _httpClient.GetAsync(productUrl);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Product {ItemId} is available.", itemId);
                    return true; // Product exists and is available
                }
                else
                {
                    _logger.LogWarning("Product {ItemId} is unavailable. Status: {StatusCode}", itemId, response.StatusCode);
                    return false; // Product does not exist or is unavailable
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for product {ItemId}", itemId);
                return false; // Treat errors as "unavailable"
            }
        }

        public async Task<bool> DeleteCatalogAsync(int sponsorId)
        {
            using var scope = _serviceProvider.CreateScope(); // Create a scope for resolving scoped services
            var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();

            var rowsAffected = await context.Database.ExecuteSqlRawAsync(
                "CALL sp_DeleteSponsorCatalog(@SponsorID)",
                new MySqlParameter("@SponsorID", sponsorId));

            _logger.LogInformation("Catalog deleted for sponsor {SponsorId}. Rows affected: {RowsAffected}", sponsorId, rowsAffected);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateCatalogAsync(int sponsorId)
        {
            using var scope = _serviceProvider.CreateScope(); // Create a scope for resolving scoped services
            var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();

            var rowsAffected = await context.Database.ExecuteSqlRawAsync(
                "CALL sp_UpdateSponsorCatalog(@SponsorID)",
                new MySqlParameter("@SponsorID", sponsorId));

            _logger.LogInformation("Catalog updated for sponsor {SponsorId}. Rows affected: {RowsAffected}", sponsorId, rowsAffected);
            return rowsAffected > 0;
        }

        public async Task<List<Products>> GetSponsorCatalogAsync(int userId)
        {
           try{ 
            using var scope = _serviceProvider.CreateScope(); // Create a scope for resolving scoped services
            var context = scope.ServiceProvider.GetRequiredService<AppDBContext>();
            var sponsorId = await context.SponsorUsers
                    .Where(su=> su.UserID == userId)
                    .Select(su => su.SponsorID)
                    .SingleOrDefaultAsync();

            var products = await context.Products
                .FromSqlRaw("CALL sp_GetSponsorCatalog(@SponsorID)", new MySqlParameter("@SponsorID", sponsorId))
                .ToListAsync();

             // Log the results
        if (products.Count == 0)
        {
            _logger.LogWarning("No products found for SponsorID: {SponsorID}", sponsorId);
        }

        return products;
           }
          catch (Exception ex)
    {
        _logger.LogError(ex, "Error fetching catalog for {userId}", userId);
        throw;
    }
        }

        public string ShortenTitle(string originalTitle)
        {
            if (string.IsNullOrEmpty(originalTitle)) return string.Empty;

            // words to ignore or remove
            var ignoreWords = new[] { "for", "accessories", "with", "and", "the", "new" };

            // Split the title into words and remove ignored ones
            var keywords = originalTitle
                .Split(' ')
                .Where(word => !ignoreWords.Contains(word.ToLower()))
                .ToList();

            // Limit the result to a maximum of 8 meaningful words...
            var shortenedTitle = string.Join(" ", keywords.Take(8));

            // Ensure it's not overly long
            if (shortenedTitle.Length > 70)
            {
                shortenedTitle = shortenedTitle.Substring(0, 67) + "...";
            }

            return shortenedTitle;
        }

        // private static string ParseDescription(ItemSummary item)
        // {

        //     // Extract relevant fields from api response
        //     var title = item.Title;
        //     var condition = item.Condition ?? "Condition unspecified";
        //     var discount = string.Empty;
        //     var price = $"{item.Price.Value} {item.Price.Currency}";

        //     if (item?.MarketingPrice != null){
        //         var discountValue = item.MarketingPrice.DiscountAmount?.Value ?? 0;
        //         var priceTreatment = item.MarketingPrice.PriceTreatment?.ToLower() ?? "special";
        //         var discountPercentage = item.MarketingPrice.DiscountPercentage;
        //             if (discountValue > 0){
        //                 discount = $"Now {priceTreatment}! Save {discountPercentage}%."; 
        //             }
        //      }

        //     var categories = string.Join(", ", item.Categories?.Select(c => c.CategoryName) ?? new[] { "Miscellaneous" });
        //     var categoryFocus = categories.Split(',').SingleOrDefault(); // Use main category

        //     // Build the description
        //     return $"{title}: {condition}. Ideal for {categoryFocus}. Priced at {price}. {discount}".Trim();
        // }


        // classes for strong-typing ebay api response
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
            [JsonProperty("itemId")]
            public required string ItemId { get; set; }

            [JsonProperty("title")]
            public required string Title { get; set; }

            [JsonProperty("categories")]
            public List<Category> Categories { get; set; } = new();

            [JsonProperty("image")]
            public required Image Image { get; set; }

            [JsonProperty("price")]
            public required Price Price { get; set; }

            [JsonProperty("marketingPrice")]
            public MarketingPrice? MarketingPrice { get; set; } // Optional, as not all items may have discounts.

            [JsonProperty("condition")]
            public string Condition { get; set; } = "Unknown"; // Default to "Unknown" if not present.
        }

        public class Category
        {
            [JsonProperty("categoryId")]
            public string CategoryId { get; set; } = string.Empty;

            [JsonProperty("categoryName")]
            public string CategoryName { get; set; } = string.Empty;
        }

        public class MarketingPrice
        {
            [JsonProperty("originalPrice")]
            public Price? OriginalPrice { get; set; }

            [JsonProperty("discountPercentage")]
            public string? DiscountPercentage { get; set; }

            [JsonProperty("discountAmount")]
            public Price? DiscountAmount { get; set; }

            [JsonProperty("priceTreatment")]
            public string? PriceTreatment { get; set; }
        }


        public class Image
        {
            [JsonProperty("imageUrl")]
            public required string ImageUrl { get; set; }
        }

        public class Price
        {
            [JsonProperty("value")]
            public required decimal Value { get; set; }

            [JsonProperty("currency")]
            public required string Currency { get; set; }
        }
    }
}
