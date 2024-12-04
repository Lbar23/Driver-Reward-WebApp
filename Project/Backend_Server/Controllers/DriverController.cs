using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend_Server.Infrastructure;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Backend_Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Backend_Server.Controllers
{
    /// <summary>
    /// DriverController:
    /// 
    /// This controller handles driver-specific functionalities, including activity tracking, 
    /// sponsor management, point transactions, and sponsor relationships.
    ///
    /// Endpoints:
    /// 
    /// [GET]   /api/driver/activity                - Retrieves driver activity (transactions and purchases)
    /// [GET]   /api/driver/available-sponsors      - Lists sponsors not yet associated with the driver
    /// [POST]  /api/driver/apply                   - Submits a new driver application
    /// [GET]   /api/driver/status/{id}             - Retrieves the status of a specific application
    /// [POST]  /api/driver/register-sponsors       - Registers the driver with selected sponsors
    /// [GET]   /api/driver/my-sponsors             - Retrieves the driver's associated sponsors and points
    /// [GET]   /api/driver/sponsor-points/{id}     - Fetches driver's point details for a specific sponsor
    /// </summary> 
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController(UserManager<Users> userManager, 
                                  AppDBContext context,
                                  ReportService reportService,
                                  IMemoryCache cache,
                                  LoggingService loggingService) 
                                  : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly ReportService _reportService = reportService;
        private readonly AppDBContext _context = context;
        private readonly LoggingService _loggingService = loggingService;


        /********* API CALLS *********/

        [Authorize(Roles = "Driver")]
        [HttpGet("activity")]
        public async Task<IActionResult> GetDriverActivity()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Warning("UserID: N/A, Category: User, Description: No user found.");
                return Unauthorized("User not found.");
            }

            Log.Information("User found - UserID: {UserId}, Category: User, Description: Activity of {Role} found", user.Id, user.Role);

            try
            {
                // Call the stored procedure for driver's point tracking
                return (IActionResult)await _reportService.GetDriverPointTracking(user.Id,
                    sponsorId: null, 
                    startDate: null,  
                    endDate: null        
                );
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching driver activity");
                return StatusCode(500, "Error fetching data");
            }
        }

        [HttpGet("available-sponsors")]
        public async Task<IActionResult> GetAvailableSponsors()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Warning("No user found.");
                return Unauthorized("User not found.");
            }

            try
            {
                Log.Information("Current user ID: {UserId}", user.Id);

                // All Sponsors Driver is Reg'd with
                var currentSponsorIds = await _context.SponsorDrivers
                    .Where(d => d.UserID == user.Id)
                    .Select(d => d.SponsorID)
                    .ToListAsync();

                Log.Information("Current sponsor IDs: {SponsorIds}", string.Join(", ", currentSponsorIds));

                // Sponsors user isn't Reg'd with; This is the part that shows
                var availableSponsors = await _context.Sponsors
                    .Where(s => !currentSponsorIds.Contains(s.SponsorID))
                    .Select(s => new SponsorDto
                    {
                        SponsorID = s.SponsorID,
                        CompanyName = s.CompanyName,
                        PointDollarValue = s.PointDollarValue
                    })
                    .ToListAsync();

                Log.Information("Found {Count} available sponsors", availableSponsors.Count);
                foreach (var sponsor in availableSponsors)
                {
                    Log.Information("Sponsor: ID={ID}, Name={Name}, Value={Value}", sponsor.SponsorID, sponsor.CompanyName, sponsor.PointDollarValue);
                }

                return Ok(availableSponsors);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting available sponsors");
                return StatusCode(500, "An error occurred while fetching sponsors.");
            }
        }

        [HttpPost("apply-sponsors")]
        public async Task<IActionResult> ApplyWithSponsors([FromBody] List<int> sponsorIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Warning("No user found.");
                return Unauthorized("User not found.");
            }

            try
            {
                // Get existing driver record or create if doesn't exist
                var driver = await _context.Users.FirstOrDefaultAsync(d => d.Id == user.Id);
                if (driver == null)
                {
                    _context.Users.Add(driver);
                    await _context.SaveChangesAsync();
                }

                // Get existing sponsorship relationships and applications
                var existingSponsors = await _context.SponsorDrivers
                    .Where(sd => sd.UserID == user.Id)
                    .Select(sd => sd.SponsorID)
                    .ToListAsync();

                var existingApplications = await _context.DriverApplications
                    .Where(da => da.UserID == user.Id && da.Status == AppStatus.Submitted)
                    .Select(da => da.SponsorID)
                    .ToListAsync();

                var newRelationships = new List<SponsorDrivers>();
                var newApplications = new List<DriverApplications>();
                var currentDate = DateTime.UtcNow;
                var currentDateOnly = DateOnly.FromDateTime(currentDate);

                foreach (var sponsorId in sponsorIds)
                {
                    // Skip if already registered or has pending application
                    if (existingSponsors.Contains(sponsorId) || existingApplications.Contains(sponsorId))
                        continue;

                    // Get the sponsor entity
                    var sponsor = await _context.Sponsors
                        .FirstOrDefaultAsync(s => s.SponsorID == sponsorId);

                    if (sponsor == null)
                    {
                        return BadRequest($"Sponsor with ID {sponsorId} not found.");
                    }

                    // Create sponsor-driver relationship
                    var sponsorDriver = new SponsorDrivers
                    {
                        UserID = user.Id,
                        SponsorID = sponsorId,
                        Points = 0,
                        User = user,
                        Sponsor = sponsor,
                        DriverPointValue = sponsor.PointDollarValue,
                        MilestoneLevel = sponsor.MilestoneThreshold == 0 ? 0 : 1
                    };
                    newRelationships.Add(sponsorDriver);

                    // Create corresponding application
                    var application = new DriverApplications
                    {
                        UserID = user.Id,
                        SponsorID = sponsorId,
                        Status = AppStatus.Submitted,
                        ApplyDate = currentDateOnly,
                        LastModified = currentDate,
                        SponsorDriver = sponsorDriver
                    };
                    newApplications.Add(application);
                }

                if (newRelationships.Any())
                {
                    // Add new relationships and applications
                    await _context.SponsorDrivers.AddRangeAsync(newRelationships);
                    await _context.DriverApplications.AddRangeAsync(newApplications);
                    await _context.SaveChangesAsync();

                    // Log applications
                    foreach (var app in newApplications)
                    {
                        Log.Information(
                            "New driver application created - UserID: {UserId}, SponsorID: {SponsorId}", 
                            app.UserID, 
                            app.SponsorID);
                    }
                }

                await _loggingService.LogAccountActivityAsync(
                    user.Id,
                    ActivityType.UpdateProfile,
                    $"Applied to sponsors: {string.Join(", ", sponsorIds)}"
                );

                return Ok(new { 
                    message = "Successfully submitted applications to selected sponsors.",
                    applicationsSubmitted = newApplications.Count
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error submitting driver applications");
                return StatusCode(500, "An error occurred while submitting applications.");
            }
        }
        
        //status of application
        [HttpGet("status/{id}")]
        public async Task<IActionResult> GetApplicationStatus(int id)
        {
            var application = await _context.DriverApplications.FindAsync(id);
            if (application == null)
            {
                return NotFound("Application not found.");
            }
            return Ok(application.Status);
        }
        
        [HttpGet("my-sponsors")]
        public async Task<IActionResult> GetDriverSponsors()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }// Get all sponsor relationships for this driver

            return await ExecuteQueryWithRetryAsync(async () =>
            {
                string cacheKey = $"driver_sponsors_{user.Id}";
                
                return await GetCachedAsync<ActionResult>(cacheKey, async () =>
                {
                    try
                    {
                        var sponsorPoints = await _context.SponsorDrivers
                            .Where(sd => sd.UserID == user.Id)
                            .Join(_context.Sponsors,
                                sd => sd.SponsorID,
                                sp => sp.SponsorID,
                                (sd, sp) => new { sd, sp })
                            .Join(_context.Sponsors,
                                ssu => ssu.sp.SponsorID,
                                s => s.SponsorID,
                                (ssu, s) => new
                                {
                                    sponsorId = s.SponsorID,
                                    sponsorName = s.CompanyName,
                                    totalPoints = ssu.sd.Points,
                                    pointDollarValue = s.PointDollarValue
                                })
                            .ToListAsync();

                            await _loggingService.LogAccountActivityAsync(
                                user.Id,
                                ActivityType.UpdateProfile,
                                "Retrieved full sponsor list"
                            );

                        return Ok(sponsorPoints);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error fetching driver's sponsors");
                        return StatusCode(500, "An error occurred while fetching sponsor data.");
                    }
                }, TimeSpan.FromMinutes(5));
            });
        }

        [HttpGet("sponsor-points/{sponsorId}")]
        public async Task<IActionResult> GetDriverSponsorPoints(int sponsorId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            try
            {
                var sponsorPoints = await _context.SponsorDrivers
                    .Where(sd => sd.UserID == user.Id && sd.SponsorID == sponsorId)
                    .Join(_context.SponsorUsers, //Again; added join where needed
                        sd => sd.SponsorID,
                        su => su.SponsorID,
                        (sd, su) => new { sd, su })
                    .Join(_context.Sponsors,
                        ssu => ssu.su.SponsorID,
                        s => s.SponsorID,
                        (ssu, s) => new
                        {
                            sponsorId = s.SponsorID,
                            sponsorName = s.CompanyName,
                            totalPoints = ssu.sd.Points,
                            pointDollarValue = s.PointDollarValue,
                            pointsHistory = _context.PointTransactions
                                .Where(pt => pt.UserID == user.Id && pt.SponsorID == sponsorId)
                                .OrderByDescending(pt => pt.TransactionDate)
                                .Take(10)
                                .Select(pt => new
                                {
                                    date = pt.TransactionDate,
                                    points = pt.PointsChanged,
                                    reason = pt.Reason
                                })
                                .ToList()
                        })
                    .FirstOrDefaultAsync();

                if (sponsorPoints == null)
                {
                    return NotFound("Sponsor relationship not found.");
                }

                await _loggingService.LogAccountActivityAsync(
                    user.Id,
                    ActivityType.UpdateProfile,
                    $"Retrieved points for sponsor ID: {sponsorId}"
                );

                return Ok(sponsorPoints);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching driver's points for sponsor");
                return StatusCode(500, "An error occurred while fetching points data.");
            }
        }

    }

}