using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SponsorController(UserManager<Users> userManager, AppDBContext context) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;

        [HttpGet("drivers")]
        public async Task<IActionResult> GetDrivers([FromQuery] string sortBy = "points", [FromQuery] string sortOrder = "desc")
        {
            var sponsorId = await _userManager.GetUserAsync(User);
            if (sponsorId == null)
            {
                return Unauthorized("User not found.");
            }

            var query = _context.Drivers.Where(d => d.UserID == sponsorId.Id);

            //Apply sorting -- currently bugged until models match, but whatever
            query = sortBy.ToLower() switch
            {
                //"name" when sortOrder.ToLower() == "asc" => query.OrderBy(d => d.Name),
                //"name" when sortOrder.ToLower() == "desc" => query.OrderByDescending(d => d.Name),
                "points" when sortOrder.ToLower() == "asc" => query.OrderBy(d => d.TotalPoints),
                _ => query.OrderByDescending(d => d.TotalPoints)
            };

            var drivers = await query.Select(d => new DriverListDto
            {
                UserID = d.UserID,
                //Name = d.Name,
                TotalPoints = d.TotalPoints,
                //City = d.City,
                //State = d.State
            }).ToListAsync();

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
    }

    public record DriverListDto
    {
        public int UserID { get; init; }
        public string? Name { get; init; } //can't be required yet until database and models are updated
        public int TotalPoints { get; init; }
        public string? City { get; init; }
        public string? State { get; init; }
    }
}