using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Serilog;
using Microsoft.Extensions.Caching.Memory;
using Backend_Server.Infrastructure;
using System.Collections.Frozen;
using System.Collections.Immutable;
using Backend_Server.Services;
using Newtonsoft.Json;
using System.Text.Json;


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
    public class SponsorController(UserManager<Users> userManager, AppDBContext context, IMemoryCache cache, LoggingService loggingService) : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;
        private readonly LoggingService _loggingService = loggingService;

        /********* API CALLS *********/

        [HttpGet("list-all")]
        public async Task<IActionResult> GetSponsors()
        {
            try
            {
                // Fetch all sponsors from the database
                var sponsors = await _context.Sponsors.ToListAsync();

                // Check if the sponsors list is empty
                if (sponsors.Count == 0)
                {
                    Log.Warning("Category: Sponsor, Description: No sponsors found in the database.");
                    return NotFound("No sponsors found");
                }


                // Return the list of sponsors
                return Ok(sponsors.ToList());
            }
            catch (Exception ex)
            {
                // Log the error
                Log.Error(ex, "An error occurred while retrieving sponsors.");
                return StatusCode(500, "An error occurred while processing your request. Please try again later.");
            }
        }
 

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

            await _loggingService.LogAccountActivityAsync(
                currentUser.Id,
                ActivityType.AccountUpdated, 
                $"Retrieved {drivers.Count} drivers"
            );
            if (drivers == null)
            {
                Log.Warning("UserID: N/A, Category: User, Description: No drivers found for sponsor with ID: {SponsorId}", sponsorUser.SponsorID);
                await _loggingService.LogAccountActivityAsync(
                    currentUser.Id,
                    ActivityType.AccountUpdated,
                    "No drivers found"
                );
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
                    DriverName = _context.Users
                        .Where(u => u.Id == app.UserID)
                        .Select(u => $"{u.FirstName} {u.LastName}")
                        .FirstOrDefault() ?? "Unknown",
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
public async Task<IActionResult> ProcessApplication(int appID, [FromBody] JsonElement payload)
{
    try
    {
        // Log the incoming payload for debugging
        Log.Information("Received Payload: {Payload}", payload.GetRawText());

        // Retrieve the application from the database
        var application = await _context.DriverApplications.FindAsync(appID);
        if (application == null)
        {
            return NotFound("Application not found.");
        }

        // Deserialize only the fields in the payload
        var status = payload.TryGetProperty("status", out var statusProp) ? (AppStatus)statusProp.GetInt32() : application.Status;
        var processReason = payload.TryGetProperty("processReason", out var reasonProp) 
            ? (ProcessedReason?)reasonProp.GetInt32() 
            : application.ProcessReason;
        var processedDate = payload.TryGetProperty("processedDate", out var dateProp) 
            ? DateOnly.Parse(dateProp.GetString()) 
            : application.ProcessedDate;
        var comments = payload.TryGetProperty("comments", out var commentsProp) 
            ? commentsProp.GetString() 
            : application.Comments;

        // Update the application with the new values
        application.Status = status;
        application.ProcessReason = processReason;
        application.ProcessedDate = processedDate;
        application.Comments = comments;
        application.LastModified = DateTime.Now; // Always update the last modified timestamp

        // Save the changes to the database
        await _context.SaveChangesAsync();

        return Ok("Application processed successfully.");
    }
    catch (Exception ex)
    {
        // Log the error for debugging
        Log.Error(ex, "Error processing application with ID {ApplicationID}", appID);
        return StatusCode(500, "Internal server error.");
    }
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
        public async Task<IActionResult> UpdateDriverPoints(int driverId, [FromBody] int newPoints)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized("User not found.");
                }

                // Get sponsor's information
                var sponsorUser = await _context.SponsorUsers
                    .Include(su => su.Sponsor)
                    .FirstOrDefaultAsync(su => su.UserID == currentUser.Id);
                    
                if (sponsorUser == null || sponsorUser.Sponsor == null)
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
                        sd.SponsorID == sponsorUser.SponsorID && 
                        sd.UserID == driverId);

                if (sponsorDriver == null)
                {
                    return NotFound($"No relationship found with Driver {driverId}");
                }

                // Update the points
                sponsorDriver.Points = newPoints;
                await _context.SaveChangesAsync();

                // Calculate dollar value based on sponsor's point-dollar ratio
                decimal dollarValue = newPoints * sponsorUser.Sponsor.PointDollarValue;

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
                    .Include(s => s.Sponsor)
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
