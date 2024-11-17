using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Serilog;
using Microsoft.Extensions.Caching.Memory;
using Backend_Server.Infrastructure;

namespace Backend_Server.Controllers
{
    /// <summary>
    /// SponsorController:
    /// 
    /// This controller manages sponsor-specific functionalities, including retrieving associated drivers,
    /// managing driver applications, and processing application statuses.
    ///
    /// Endpoints:
    /// 
    /// [GET]  /api/sponsor/drivers                 - Retrieves a list of drivers associated with the sponsor
    /// [GET]  /api/sponsor/drivers/{id}           - Retrieves detailed information about a specific driver
    /// [GET]  /api/sponsor/applications           - Retrieves all driver applications submitted to the sponsor
    /// [POST] /api/sponsor/applications/{id}/process - Approves or rejects a specific driver application
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorController(UserManager<Users> userManager, AppDBContext context, IMemoryCache cache) : CachedBaseController(cache)
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

                    Log.Information("User ID: {UserId}", currentUser.Id);
                    
                    // Query drivers for this sponsor
                    var drivers = await _context.SponsorDrivers
                        .Where(sd => sd.SponsorID == sponsor.SponsorID)
                        .Join(
                            _context.Users,
                            sd => sd.DriverID,
                            u => u.Id,
                            (sd, u) => new DriverListDto
                            {
                                UserID = u.Id,
                                Name = u.UserName,
                                Email = u.Email,
                                TotalPoints = sd.Points
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
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null){
                return Unauthorized("User not found.");
            }

            var sponsor = await _context.Sponsors
                .FirstOrDefaultAsync(s => s.UserID == currentUser.Id);
            if (sponsor == null){
                return NotFound("Sponsor not found.");
            }

            var driverInfo = await _context.SponsorDrivers
                .Where(sd => sd.SponsorID == sponsor.SponsorID && sd.DriverID == id)
                .Join(
                    _context.Users,
                    sd => sd.DriverID,
                    u => u.Id,
                    (sd, u) => new DriverListDto
                    {
                        UserID = u.Id,
                        Name = u.UserName,
                        Email = u.Email,
                        TotalPoints = sd.Points
                    })
                .FirstOrDefaultAsync();

            if (driverInfo == null){
                return NotFound("Driver not found.");
            }

            return Ok(driverInfo);
        }

        //This method can be separated into two parts; just put one here as reference
        // [HttpPost("drivers/points/add-or-deduct")]
        // public async Task<IActionResult> AddOrDeductDriverPoints(int driverId, int pointsToAdd)
        // {
        //     return Ok(pointsToAdd);
        // }

        [HttpGet("applications")]
        public async Task<IActionResult> GetDriverApplications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            // Get sponsor's ID
            var sponsor = await _context.Sponsors.FirstOrDefaultAsync(s => s.UserID == currentUser.Id);
            if (sponsor == null)
            {
                Log.Warning("No sponsor found for User ID: {UserId}", currentUser.Id);
                return NotFound("Sponsor information not found.");
            }

            // Get all applications for the sponsor, regardless of status
            var applications = await _context.DriverApplications
                .Where(app => app.SponsorID == sponsor.SponsorID)
                .Select(app => new
                {
                    app.ApplicationID,
                    app.UserID,
                    app.ApplyDate,
                    app.Status,
                    app.Reason
                })
                .ToListAsync();

            Log.Information("Found {Count} applications for sponsor", applications.Count);
            if (applications.Count == 0)
            {
                return NotFound("No applications found.");
            }

            return Ok(applications);
        }

        [HttpPost("applications/{applicationID}/process")]
        public async Task<IActionResult> ProcessApplication(int applicationID, [FromQuery] string action)
        {
            var application = await _context.DriverApplications.FindAsync(applicationID);
            if (application == null)
            {
                return NotFound("Application not found.");
            }

            if (action == "approve")
            {
                application.Status = AppStatus.Approved;
                application.ProcessedDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else if (action == "reject")
            {
                application.Status = AppStatus.Rejected;
                application.ProcessedDate = DateOnly.FromDateTime(DateTime.Now);
            }
            else
            {
                return BadRequest("Invalid action specified. Use 'approve' or 'reject'.");
            }

            await _context.SaveChangesAsync();
            return Ok($"Application {action}ed successfully.");
        }
        // [HttpGet("reports")] //Same here as Admin reports...split into smaller async tasks
        // public async Task<IActionResult> GetReports()
        // {
        //     return Ok();
        // }

        // [HttpGet("products")]
        // public async Task<IActionResult> GetProducts()
        // {
        //     return Ok();
        // }

        // [HttpGet("products/{id}")]
        // public async Task<IActionResult> GetProduct(int id)
        // {
        //     return Ok();
        // }


    public record DriverListDto
    {
        public int UserID { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public int TotalPoints { get; init; }
        // public string? City { get; init; }
        // public string? State { get; init; }
    }
}}
