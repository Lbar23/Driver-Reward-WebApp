using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Server.Services;
using static Backend_Server.Services.CatalogService;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Backend_Server.Models;

namespace Backend_Server.Controllers
{
    /// <summary>
    /// CatalogController:
    /// Provides endpoints for managing and retrieving product catalog data from eBay.
    /// 
    /// Endpoints:
    /// - [POST]   /api/catalog/create                          - Creates a catalog for a sponsor.
    /// - [GET]    /api/catalog/{sponsorId}                     - Retrieves a sponsor's catalog.
    /// - [PUT]    /api/catalog/update/{sponsorId}              - Updates an existing catalog for a sponsor.
    /// - [DELETE] /api/catalog/delete/{sponsorId}              - Deletes a sponsor's catalog.
    /// - [GET]    /api/catalog/availability/{productId}        - Checks availability of a specific product.
    /// - [POST]   /api/catalog/create-purchase                 - Creates a purchase and updates points.
    /// - [PUT]    /api/catalog/update-purchase/{purchaseID}    - Updates a purchase.
    /// - [POST]   /api/catalog/refund-purchase/{purchaseID}    - Refunds a purchase.
    /// - [POST]   /api/catalog/cancel-purchase/{purchaseID}    - Cancels a purchase.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController(CatalogService catalogService, 
                                   AppDBContext context, 
                                   UserManager<Users> userManager) 
                                   : ControllerBase
    {
        private readonly CatalogService _catalogService = catalogService;
        private readonly AppDBContext _context = context;
        private readonly UserManager<Users> _userManager = userManager;

        /// <summary>
        /// Creates a catalog for a sponsor by fetching products from eBay.
        /// </summary>
        /// <param name="sponsorId">ID of the sponsor for whom the catalog is being created.</param>
        /// <param name="categories">List of eBay categories to include in the catalog.</param>
        /// <param name="numberOfProducts">Number of products to retrieve per category.</param>
        /// <param name="pointValue">Conversion rate of product price to points.</param>
        /// <returns>A list of products added to the catalog.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateCatalog(
            int sponsorId,
            [FromQuery] List<string> categories,
            [FromQuery] int numberOfProducts,
            [FromQuery] int pointValue)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;
            var parsedCategories = new List<EbayCategory>();
            foreach (var category in categories){
                
                if (Enum.TryParse<EbayCategory>(category, true, out var ebayCategory)){
                    parsedCategories.Add(ebayCategory);
                }

                else{
                    return BadRequest($"Invalid category: {category}");
                }
            }

            if (!parsedCategories.Any())
            {
                return BadRequest("At least one valid category must be provided.");
            }

            var products = await _catalogService.CreateCatalogAsync( parsedCategories.ToString(), numberOfProducts, userId);
            if (!products.Any())
            {
                return BadRequest("Failed to create catalog.");
            }

            return Ok(products);
        }

        /// <summary>
        /// Retrieves the catalog for a specific sponsor.
        /// </summary>
        /// <param name="userId">ID of the user whose catalog is being retrieved.</param>
        /// <returns>A list of products in the sponsor's catalog.</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetSponsorCatalog(int userId)
        {
            var catalog = await _catalogService.GetSponsorCatalogAsync(userId);
            return catalog != null && catalog.Any()
                ? Ok(catalog)
                : NotFound("No catalog found for the sponsor.");
        }

        /// <summary>
        /// Updates the catalog for a specific sponsor.
        /// </summary>
        /// <param name="sponsorId">ID of the sponsor whose catalog is being updated.</param>
        /// <returns>Success or failure of the update operation.</returns>
        [HttpPut("update/{sponsorId}")]
        public async Task<IActionResult> UpdateCatalog(int sponsorId)
        {
            var success = await _catalogService.UpdateCatalogAsync(sponsorId);
            return success
                ? Ok("Catalog updated successfully.")
                : BadRequest("Failed to update catalog.");
        }

        /// <summary>
        /// Deletes the catalog for a specific sponsor.
        /// </summary>
        /// <param name="sponsorId">ID of the sponsor whose catalog is being deleted.</param>
        /// <returns>Success or failure of the delete operation.</returns>
        [HttpDelete("delete/{sponsorId}")]
        public async Task<IActionResult> DeleteCatalog(int sponsorId)
        {
            var success = await _catalogService.DeleteCatalogAsync(sponsorId);
            return success
                ? Ok("Catalog deleted successfully.")
                : BadRequest("Failed to delete catalog.");
        }

        /// <summary>
        /// Checks the availability of a specific product by product ID.
        /// </summary>
        /// <param name="productId">The product ID to check availability for.</param>
        /// <returns>Availability status of the product.</returns>
        [HttpGet("availability/{productId}")]
        public async Task<IActionResult> CheckProductAvailability(string productId)
        {
            var isAvailable = await _catalogService.CheckProductAvailabilityAsync(productId);
            return Ok(new { ProductID = productId, IsAvailable = isAvailable });
        }

        /// <summary>
        /// Checks the availability of all products in a request before creating a purchase.
        /// </summary>
        /// <param name="request">Purchase request data.</param>
        /// <returns>Success or failure of the operation.</returns>
        [HttpPost("create-purchase")]
        public async Task<IActionResult> CreatePurchase([FromBody] PurchaseRequest request)
        {
            try
            {
                // Validate product availability
                foreach (var product in request.Products)
                {
                    var isAvailable = await _catalogService.CheckProductAvailabilityAsync(product.ProductID.ToString());
                    if (!isAvailable)
                    {
                        return BadRequest(new { error = $"Product {product.ProductID} is not available." });
                    }
                }

                // Serialize products to JSON
                var productsJson = JsonConvert.SerializeObject(request.Products);

                // Call stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                    @"CALL sp_CreatePurchaseWithProducts({0}, {1}, {2}, {3}, {4}, {5})",
                    request.SponsorID,
                    request.UserID,
                    request.TotalPointsSpent,
                    request.PurchaseDate,
                    (int)request.Status,
                    productsJson);

                return Ok(new { message = "Purchase created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing purchase, ensuring products are available and updating purchase details.
        /// </summary>
        /// <param name="purchaseID">ID of the purchase to update.</param>
        /// <param name="request">Updated purchase details.</param>
        /// <returns>Success or failure of the operation.</returns>
        [HttpPut("update-purchase/{purchaseID}")]
        public async Task<IActionResult> UpdatePurchase(int purchaseID, [FromBody] UpdatePurchase request)
        {
            try
            {
                // Validate product availability
                foreach (var product in request.UpdatedProducts)
                {
                    var isAvailable = await _catalogService.CheckProductAvailabilityAsync(product.ProductID.ToString());
                    if (!isAvailable)
                    {
                        return BadRequest(new { error = $"Product {product.ProductID} is not available." });
                    }
                }

                // Serialize updated products to JSON
                var updatedProductsJson = JsonConvert.SerializeObject(request.UpdatedProducts);

                // Call stored procedure
                await _context.Database.ExecuteSqlRawAsync(
                    @"CALL sp_UpdatePurchase({0}, {1}, {2})",
                    purchaseID,
                    updatedProductsJson,
                    request.UpdatedPointsSpent);

                return Ok(new { message = "Purchase updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancels a purchase, ensuring it is not already refunded or completed.
        /// </summary>
        /// <param name="purchaseID">ID of the purchase to cancel.</param>
        /// <returns>Success or failure of the operation.</returns>
        [HttpPost("cancel-purchase/{purchaseID}")]
        public async Task<IActionResult> CancelPurchase(int purchaseID)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"CALL sp_CancelPurchase({0})",
                    purchaseID);

                return Ok(new { message = "Purchase canceled successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Refunds a purchase, restoring points to the driver.
        /// </summary>
        /// <param name="purchaseID">ID of the purchase to refund.</param>
        /// <returns>Success or failure of the operation.</returns>
        [HttpPost("refund-purchase/{purchaseID}")]
        public async Task<IActionResult> RefundPurchase(int purchaseID)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"CALL sp_RefundPurchase({0})",
                    purchaseID);

                return Ok(new { message = "Purchase refunded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
