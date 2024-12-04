using Backend_Server;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Backend_Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using static Backend_Server.Services.CatalogService;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly CatalogService _catalogService;
    private readonly AppDBContext _context;
    private readonly UserManager<Users> _userManager;

    public CatalogController(
        CatalogService catalogService,
        AppDBContext context,
        UserManager<Users> userManager)
    {
        _catalogService = catalogService;
        _context = context;
        _userManager = userManager;
    }

    /// <summary>
    /// Creates a catalog for a sponsor by fetching products from eBay.
    /// Processes each category individually to handle eBay API limitations.
    /// </summary>
    /// <param name="sponsorId">ID of the sponsor for whom the catalog is being created.</param>
    /// <param name="category">The eBay category to fetch products from.</param>
    /// <param name="numberOfProducts">Number of products to retrieve per category.</param>
    /// <param name="pointValue">Conversion rate of product price to points.</param>
    /// <returns>A list of products added to the catalog.</returns>
   [HttpPost("create")]
public async Task<IActionResult> CreateCatalog(
    int sponsorId,
    [FromQuery] string category, // Single category as a string
    [FromQuery] int numberOfProducts,
    [FromQuery] int pointValue)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return Unauthorized("User not authenticated.");
    }

    var userId = user.Id;

    // Convert the single category string into a List<string>
    var categories = new List<string> { category };

    var products = await _catalogService.CreateCatalogAsync(categories, numberOfProducts, userId);

    if (!products.Any())
    {
        return BadRequest("Failed to create catalog. No products retrieved.");
    }

    var productDtos = products.Select(product => new ProductDto
    {
        SponsorID = product.SponsorID,
        ProductID = product.ProductID,
        ProductName = product.ProductName,
        Category = product.Category,
        Description = product.Description,
        CurrencyPrice = product.CurrencyPrice,
        PriceInPoints = (int)(product.CurrencyPrice * pointValue),
        ExternalID = product.ExternalID,
        ImageUrl = product.ImageUrl,
        Availability = product.Availability
    });

    return Ok(productDtos);
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
}
