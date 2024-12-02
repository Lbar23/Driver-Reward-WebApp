using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
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
    /// [PUT]   /api/sponsor/update/{id}                - Updates the driver application with a sponsor decision
    /// [GET]   /api/sponsor/drivers                    - Retrieves a list of drivers associated with the sponsor
    /// [GET]   /api/sponsor/drivers/{id}               - Retrieves detailed information about a specific driver
    /// [GET]   /api/sponsor/applications               - Retrieves all driver applications submitted to the sponsor
    /// [POST]  /api/sponsor/applications/{id}/process  - Approves or rejects a specific driver application
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorController(UserManager<Users> userManager, AppDBContext context, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;

        /********* API CALLS *********/

        // update applicaiton
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] DriverApplications updatedApplication)
        {
            var application = await _context.DriverApplications.FindAsync(id);
            if (application == null)
            {
                return NotFound("Application not found.");
            }

            application.Status = updatedApplication.Status;
            application.ProcessedDate = updatedApplication.ProcessedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            application.ProcessReason = updatedApplication.ProcessReason;
            _context.DriverApplications.Update(application);
            await _context.SaveChangesAsync();
            return Ok("Application updated successfully!");
        }

        //pending
        [HttpGet("pending")]
         public async Task<IActionResult> GetPendingApplications()
        {
            var pendingApplications = await _context.DriverApplications
                .Where(app => app.Status == AppStatus.Submitted)
                .ToListAsync();
            return Ok(pendingApplications);
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            // Get sponsor's ID
            var sponsorUser = await _context.SponsorUsers
                .FirstOrDefaultAsync(su => su.UserID == currentUser.Id);
                
            if (sponsorUser == null)
            {
                Log.Warning("UserID: {UserId}, Category: System, Description: No sponsor found for User ID: {UserId}", currentUser.Id, currentUser.Id);
                return NotFound("Sponsor information not found.");
            }

            Log.Information("User ID: {UserId}", currentUser.Id);
            
            // Query drivers for this sponsor
            var drivers = await _context.SponsorDrivers
                .Where(sd => sd.SponsorID == sponsorUser.SponsorID)
                .Join(
                    _context.Users,
                    sd => sd.UserID,
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
                Log.Warning("UserID: N/A, Category: User, Description: No drivers found for sponsor with ID: {SponsorId}", sponsorUser.SponsorID);
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

            var sponsorUser = await _context.SponsorUsers
                .FirstOrDefaultAsync(su => su.UserID == currentUser.Id);
            if (sponsorUser == null){
                return NotFound("Sponsor not found.");
            }

            var driverInfo = await _context.SponsorDrivers
                .Where(sd => sd.SponsorID == sponsorUser.SponsorID && sd.UserID == id)
                .Join(
                    _context.Users,
                    sd => sd.UserID,
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

        [HttpGet("applications")]
        public async Task<IActionResult> GetDriverApplications()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            // Get sponsor's ID
            var sponsorUser = await _context.SponsorUsers.FirstOrDefaultAsync(su => su.UserID == currentUser.Id);
            if (sponsorUser == null)
            {
                Log.Warning("No sponsor found for User ID: {UserId}", currentUser.Id);
                return NotFound("Sponsor information not found.");
            }

            // Get all applications for the sponsor, regardless of status
            var applications = await _context.DriverApplications
                .Where(app => app.SponsorID == sponsorUser.SponsorID)
                .Select(app => new
                {
                    app.ApplicationID,
                    app.UserID,
                    app.ApplyDate,
                    app.Status,
                    app.ProcessReason
                })
                .ToListAsync();

            Log.Information("Found {Count} applications for sponsor", applications.Count);
            if (applications.Count == 0)
            {
                return NotFound("No applications found.");
            }

            return Ok(applications);
        }

        [HttpPost("applications/{appID}")]
        public async Task<IActionResult> ProcessApplication(int appID, [FromQuery] string action)
        {

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("User not found.");
            }

            var application = await _context.DriverApplications.FindAsync(appID);
            if (application == null)
            {
                return NotFound("Application not found.");
            }
            var sponsorUser = await _context.SponsorUsers
                .FirstOrDefaultAsync(su => su.UserID == currentUser.Id && su.SponsorID == application.SponsorID);

            if (sponsorUser == null){
                return Unauthorized("Sponsor user does not have access to this application.");
            }

            if (action == "approve"){
                application.Status = AppStatus.Approved;
                application.ProcessedDate = DateOnly.FromDateTime(DateTime.Now);
            }

            else if (action == "reject"){
                application.Status = AppStatus.Rejected;
                application.ProcessedDate = DateOnly.FromDateTime(DateTime.Now);
            }

            else{
                return BadRequest("Invalid action specified. Use 'approve' or 'reject'.");
            }

            await _context.SaveChangesAsync();
            return Ok($"Application {action}ed successfully.");
        }
        [HttpPut("point-value")]
        public async Task<IActionResult> UpdatePointDollarValue([FromBody] decimal newPointValue)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                // Get sponsor's ID
                var sponsor = await _context.SponsorUsers
                    .FirstOrDefaultAsync(s => s.UserID == currentUser.Id);
                if (sponsor == null)
                {
                    Log.Warning("No sponsor found for User ID: {UserId}", currentUser.Id);
                    return NotFound("Sponsor information not found.");
                }

                // Input validation
                if (newPointValue <= 0)
                {
                    return BadRequest("Point value must be greater than 0");
                }

                // Update the point-dollar value
                sponsor.Sponsor.PointDollarValue = newPointValue;
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Point-dollar value updated successfully",
                    newValue = newPointValue 
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while updating point value");
                return StatusCode(500, "An error occurred while updating point value");
            }
        }

        [HttpPut("driver/{driverId}/points")]
        public async Task<IActionResult> UpdateDriverPoints(
         int driverId, 
        [FromBody] int newPoints)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                // Get sponsor's ID
                var sponsor = await _context.SponsorUsers
                    .FirstOrDefaultAsync(s => s.UserID == currentUser.Id);
                if (sponsor == null)
                {
                    Log.Warning("No sponsor found for User ID: {UserId}", currentUser.Id);
                    return NotFound("Sponsor information not found.");
                }

                // Input validation
                if (newPoints < 0)
                {
                    return BadRequest("Points cannot be negative");
                }

                var sponsorDriver = await _context.SponsorDrivers
                    .FirstOrDefaultAsync(sd => 
                        sd.SponsorID == sponsor.SponsorID && 
                        sd.UserID == driverId);

                if (sponsorDriver == null)
                {
                    return NotFound($"No relationship found with Driver {driverId}");
                }

                // Update the points
                sponsorDriver.Points = newPoints;
                await _context.SaveChangesAsync();

                // Calculate dollar value based on sponsor's point-dollar ratio
                decimal dollarValue = newPoints * sponsor.Sponsor.PointDollarValue;

                return Ok(new { 
                    message = "Driver points updated successfully",
                    points = newPoints,
                    dollarValue = dollarValue
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating points for driver {DriverId}", driverId);
                return StatusCode(500, "An error occurred while updating driver points");
            }
        }
        [HttpGet("point-ratio")]
        public async Task<IActionResult> GetPointRatio()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                // Get sponsor's information
                var sponsor = await _context.SponsorUsers
                    .FirstOrDefaultAsync(s => s.UserID == currentUser.Id);
                if (sponsor == null)
                {
                    Log.Warning("No sponsor found for User ID: {UserId}", currentUser.Id);
                    return NotFound("Sponsor information not found.");
                }

                return Ok(new { 
                    pointDollarValue = sponsor.Sponsor.PointDollarValue,
                    sponsorId = sponsor.SponsorID
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while retrieving point ratio");
                return StatusCode(500, "An error occurred while retrieving point ratio");
            }
        }
    }
}
