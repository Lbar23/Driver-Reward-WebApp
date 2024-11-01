using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverController(UserManager<Users> userManager, SignInManager<Users> signInManager, AppDBContext context) : ControllerBase
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
                Log.Warning("UserID: N/A, Category: User, Description: No user found.");
                return Unauthorized("User not found.");
            }

            // Log user information
            Log.Information("User found - UserID: {UserId}, Category: User, Description: Activity of {UserType} found", user.Id, user.UserType);

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
            var sponsorPoints = await _context.Drivers
                .Where(d => d.UserID == user.Id)
                .Join(_context.Sponsors,
                    d => d.SponsorID,
                    s => s.SponsorID,
                    (d, s) => new PointValueDto
                    {
                        TotalPoints = d.TotalPoints,
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

        [HttpGet("points")]
        public async Task<IActionResult> GetDriverPoints()
        {
            return Ok();
        }

        [HttpGet("purchases")]
        public async Task<IActionResult> GetDriverPurchases()
        {
            return Ok();
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetDriverTransactions()
        {
            return Ok();
        }

        [HttpGet("purchase")]
        public async Task<IActionResult> GetDriverPurchase(int id)
        {
            return Ok();
        }

        [HttpPut("purchase/{id}")]
        public async Task<IActionResult> CancelPurchase(int id)
        {
            return Ok();
        }

        [HttpGet("sponsor/{id}")]
        public async Task<IActionResult> GetSponsor(int id)
        {
            return Ok();
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetDriverProfile()
        {
            return Ok();
        }
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