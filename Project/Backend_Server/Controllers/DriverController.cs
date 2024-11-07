using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend_Server.Infrastructure;
using Backend_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController(UserManager<Users> userManager, AppDBContext context, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;

        //uh, this is two stories combined, really
        //Since transactions and purchases are one, and really, getting the default value doesn't need a separate method
        //we ball with dis
        //[Authorize(Roles = "Driver")]
        [HttpGet("activity")]
        public async Task<IActionResult> GetDriverActivity()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Warning("No user found.");
                return Unauthorized("User not found.");
            }

            // Log user information
            Log.Information("User found - ID: {UserId}, Type: {UserType}", user.Id, user.UserType);

            // Now try to find our specific driver
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserID == user.Id);
            

            // Point transactions with correct sponsor info
            var pointTransactions = await _context.PointTransactions
                .Where(t => t.UserID == driver!.UserID)
                .Join(_context.Sponsors,
                    pt => pt.SponsorID,
                    s => s.SponsorID,
                    (pt, s) => new TransactionDto
                    {
                        Date = pt.TransactionDate,
                        Points = pt.PointsChanged,
                        Type = "Point Change",
                        Reason = pt.Reason,
                        SponsorName = s.CompanyName
                    })
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // Purchases with product and sponsor info
            var purchases = await _context.Purchases
                .Where(p => p.UserID == driver!.UserID)
                .Join(_context.Products,
                    pur => pur.Product.ProductID,
                    prod => prod.ProductID,
                    (pur, prod) => new { Purchase = pur, Product = prod })
                .Join(_context.Sponsors,
                    p => p.Product.SponsorID,
                    s => s.SponsorID,
                    (p, s) => new TransactionDto
                    {
                        Date = p.Purchase.PurchaseDate,
                        Points = -p.Purchase.PointsSpent,
                        Type = "Purchase",
                        Reason = $"Purchased {p.Product.Name}",
                        Status = p.Purchase.Status.ToString(),
                        SponsorName = s.CompanyName
                    })
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // Get all sponsor point values for this driver
            var sponsorPoints = await _context.SponsorDrivers
                .Where(sd => sd.DriverID == user.Id)
                .Join(_context.Sponsors,
                    sd => sd.SponsorID,
                    s => s.SponsorID,
                    (sd, s) => new PointValueDto
                    {
                        TotalPoints = sd.Points,
                        PointValue = s.PointDollarValue,
                        SponsorName = s.CompanyName
                    })
                .ToListAsync();

            // Combine and sort all transactions
            var allTransactions = pointTransactions.Concat(purchases)
                .OrderByDescending(t => t.Date)
                .ToList();

            return Ok(new
            {
                PointValues = sponsorPoints,  // Now returns points for all sponsors
                Transactions = allTransactions
            });
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
                    .Where(d => d.DriverID == user.Id)
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

        [HttpPost("register-sponsors")]
        public async Task<IActionResult> RegisterWithSponsors([FromBody] List<int> sponsorIds)
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
                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserID == user.Id);
                if (driver == null)
                {
                    driver = new Drivers { UserID = user.Id };
                    _context.Drivers.Add(driver);
                    await _context.SaveChangesAsync();
                }

                // Get existing sponsorship relationships
                var existingSponsors = await _context.SponsorDrivers
                    .Where(sd => sd.DriverID == user.Id)
                    .Select(sd => sd.SponsorID)
                    .ToListAsync();

                var newRelationships = new List<SponsorDrivers>();

                foreach (var sponsorId in sponsorIds)
                {
                    // Skip if already registered
                    if (existingSponsors.Contains(sponsorId))
                        continue;

                    // Get the sponsor entity
                    var sponsor = await _context.Sponsors
                        .FirstOrDefaultAsync(s => s.SponsorID == sponsorId);

                    if (sponsor == null)
                    {
                        return BadRequest($"Sponsor with ID {sponsorId} not found.");
                    }

                    var sponsorDriver = new SponsorDrivers
                    {
                        DriverID = user.Id,
                        SponsorID = sponsorId,
                        Points = 0,
                        Driver = driver,      
                        Sponsor = sponsor     
                    };
                    newRelationships.Add(sponsorDriver);
                }

                if (newRelationships.Any())
                {
                    await _context.SponsorDrivers.AddRangeAsync(newRelationships);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { 
                    message = "Successfully registered with selected sponsors.",
                    registeredCount = newRelationships.Count
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error registering driver with sponsors");
                return StatusCode(500, "An error occurred while registering with sponsors.");
            }
        }

        // [HttpGet("points")]
        // public async Task<IActionResult> GetDriverPoints()
        // {
        //     return Ok();
        // }

        // [HttpGet("purchases")]
        // public async Task<IActionResult> GetDriverPurchases()
        // {
        //     return Ok();
        // }

        //For now, method is synchronous so I don't get no warnings when building and running...
        //DO NOT FORGET IF IMPLEMENTATION UPDATES IN THE FUTURE
        [HttpGet("transactions")]
        public IActionResult GetDriverTransactions()
        {
            return Ok();
        }

        // [HttpGet("purchase")]
        // public async Task<IActionResult> GetDriverPurchase(int id)
        // {
        //     return Ok();
        // }

        // [HttpPut("purchase/{id}")]
        // public async Task<IActionResult> CancelPurchase(int id)
        // {
        //     return Ok();
        // }

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
                            .Where(sd => sd.DriverID == user.Id)
                            .Join(
                                _context.Sponsors,
                                sd => sd.SponsorID,
                                s => s.SponsorID,
                                (sd, s) => new
                                {
                                    sponsorId = s.SponsorID,
                                    sponsorName = s.CompanyName,
                                    totalPoints = sd.Points,
                                    pointDollarValue = s.PointDollarValue
                                })
                            .ToListAsync();

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
                    .Where(sd => sd.DriverID == user.Id && sd.SponsorID == sponsorId)
                    .Join(
                        _context.Sponsors,
                        sd => sd.SponsorID,
                        s => s.UserID,
                        (sd, s) => new
                        {
                            sponsorId = s.UserID,
                            sponsorName = s.CompanyName,
                            totalPoints = sd.Points,
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

                return Ok(sponsorPoints);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching driver's points for sponsor");
                return StatusCode(500, "An error occurred while fetching points data.");
            }
        }

        // [HttpGet("profile")]
        // public async Task<IActionResult> GetDriverProfile()
        // {
        //     return Ok();
        // }
    }

    public record SponsorDto //mainly here so Drivers can switch between different sponsors
    {
        public int SponsorID { get; init; }
        public required string CompanyName { get; init; }
        public decimal PointDollarValue { get; init; }
    }

    public record PointValueDto
    {
        public int TotalPoints { get; init; }
        public decimal PointValue { get; init; }
        public required string SponsorName { get; init; }
        public decimal TotalValue => TotalPoints * PointValue;
    }

    public record TransactionDto
    {
        public DateTime Date { get; init; }
        public int Points { get; init; }
        public required string Type { get; init; }
        public required string Reason { get; init; }
        public required string SponsorName { get; init; }
        public string? Status { get; init; }
    }
}