using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Serilog;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorController(UserManager<Users> userManager, AppDBContext context) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;

        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }
            

            // Get sponsor's ID
            var sponsor = await _context.Sponsors
                .FirstOrDefaultAsync(s => s.UserID == currentUser.Id);
            if (sponsor == null)
            {
                Log.Warning("UserID: {UserId}, Category: System, Description: No sponsor found for User ID: {UserId}", currentUser.Id, currentUser.Id);
                return NotFound("Sponsor information not found.");
            }

            // Let's check what drivers exist first
            var allDrivers = await _context.Drivers.ToListAsync();

            // Query drivers for this sponsor
            var query = _context.Drivers
                .Where(d => d.SponsorID == sponsor.SponsorID);

            var drivers = await query
                .Select(d => new DriverListDto
                {
                    UserID = d.UserID,
                    Email = currentUser.Email,
                    TotalPoints = d.TotalPoints
                })
                .ToListAsync();

            Log.Information("UserID: N/A, Category: User, Description: Found {Count} drivers for sponsor", drivers.Count);
            if (drivers == null)
            {
                Log.Warning("UserID: N/A, Category: User, Description: No drivers found for sponsor with ID: {SponsorId}", sponsor.SponsorID);
                return NotFound("No drivers found");
            }

            return Ok(drivers);
        }

        //Same here with name, address, etc. Points and Id remain the same
        [HttpGet("drivers/{id}")]
        public async Task<IActionResult> GetDriver(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var driver = await _context.Drivers
                .Where(d => d.UserID == id && d.UserID == user.Id)
                .Select(d => new DriverListDto
                {
                    UserID = d.UserID,
                    //Name = d.Name,
                    TotalPoints = d.TotalPoints,
                    //City = d.City,
                    //State = d.State
                })
                .FirstOrDefaultAsync();

            if (driver == null)
            {
                return NotFound("Driver not found.");
            }

            return Ok(driver);
        }

        //This method can be separated into two parts; just put one here as reference
        [HttpPost("drivers/points/add-or-deduct")]
        public async Task<IActionResult> AddOrDeductDriverPoints(int driverId, int pointsToAdd)
        {
            return Ok(pointsToAdd);
        }

        [HttpGet("applications")]
        public async Task<IActionResult> GetDriverApplications()
        {
            return Ok();
        }

        [HttpPost("applications/{id}/process")]
        public async Task<IActionResult> DriverApplicationProcess(string id)
        {
            return Ok();
        }

        [HttpGet("reports")] //Same here as Admin reports...split into smaller async tasks
        public async Task<IActionResult> GetReports()
        {
            return Ok();
        }

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            return Ok();
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            return Ok();
        }




        
    }

    public record DriverListDto
    {
        public int UserID { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public int TotalPoints { get; init; }
        // public string? City { get; init; }
        // public string? State { get; init; }
    }
}