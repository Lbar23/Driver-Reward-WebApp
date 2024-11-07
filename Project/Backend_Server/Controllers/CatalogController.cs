using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Server.Services;
using static Backend_Server.Services.CatalogService;
using Microsoft.Extensions.Caching.Memory;
using Backend_Server.Infrastructure;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController(CatalogService catalogService, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly CatalogService _catalogService = catalogService;

        [HttpGet("products")]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            var products = await _catalogService.GetProductsAsync();
            if (products == null || products.Count == 0)
            {
                return NotFound("No products found.");
            }
            return Ok(products); 
        }
    }
}
