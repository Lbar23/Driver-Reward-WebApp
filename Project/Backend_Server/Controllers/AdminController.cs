using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Backend_Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend_Server.Services;
using Serilog;
using Microsoft.AspNetCore.Authorization;

namespace Backend_Server.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly NotifyService _notifyService;

        public AdminController(AppDBContext context, UserManager<Users> userManager, NotifyService notifyService)
        {
            _context = context;
            _userManager = userManager;
            _notifyService = notifyService;
        }

        /// <summary>
        /// Creates a new user of any kind (admin privileges)
        /// RBAC Needed for frontend (and general call; it works, just doesn't redirect to dashboard after Logging in):
        /// -- Admin creates any Users (Admin, Driver, Sponsors) -- Guests aren't needed since they register normally
        /// -- Sponsors can also create other Sponsors (under their Company Name) and Drivers (Also under their Company Name)
        /// -- Database current has 3 Sponsors (to test insertion) with same password hash; Delete them later to actually add in rememberable passwords
        /// -- And one Admin; Admin Password is "TheOneONLY#456" for testing purposes with Login and Admin RBAC privileges later on
        /// -- Current implementation can be simplified, but this was done in like, 2 hours of API testing. There's no frontend yet; waiting for updates...
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Sponsor")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            //Later on, I can create a permissions attribute instead of only checking for roles...
            //But this is just updating the controllers first; not worrying about that up until Sprint 8-9

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the execution strategy from the context
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                // Execute the entire operation with retry strategy
                await strategy.ExecuteAsync(async () =>
                {
                    // Start transaction
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var user = new Users
                        {
                            UserName = model.Username,
                            Email = model.Email,
                            UserType = model.UserType.ToString(),
                            CreatedAt = DateTime.UtcNow,
                            EmailConfirmed = true
                        };

                        //This is where you would, essentially, generate a temp password if integrating notification SMS/Email for Sponsors or otherwise...
                        //Using Gen password...

                        var result = await _userManager.CreateAsync(user, model.Password); //<-- 
                        if (!result.Succeeded)
                            throw new InvalidOperationException(
                                string.Join(", ", result.Errors.Select(e => e.Description)));

                        //Add user to appropriate role
                        await _userManager.AddToRoleAsync(user, user.UserType);

                        //Create type-specific record
                        switch (model.UserType)
                        {
                            case UserType.Admin:
                                await _context.Admins.AddAsync(new Admins { UserID = user.Id });
                                break;

                            case UserType.Sponsor:
                                if (string.IsNullOrEmpty(model.CompanyName) || string.IsNullOrEmpty(model.SponsorType))
                                    throw new ArgumentException("CompanyName and SponsorType are required for sponsors");
                                    
                                var sponsor = new Sponsors
                                {
                                    UserID = user.Id,
                                    CompanyName = model.CompanyName,
                                    SponsorType = model.SponsorType,
                                    PointDollarValue = model.PointDollarValue ?? 0.01m
                                };
                                await _context.Sponsors.AddAsync(sponsor);

                                //Send credentials email (I'm not sure which template you're using in the future)
                                //So, edit dis later
                                await _notifyService.SendTemplateEmail(
                                    user.Email,
                                    "uh-me-when-uh-template",
                                    new Dictionary<string, string> {
                                        { "username", user.UserName },
                                        { "temporary_password", model.Password }
                                    }
                                );
                                break;

                            case UserType.Driver:
                                if (!model.SponsorID.HasValue)
                                    throw new ArgumentException("SponsorID is required for drivers");
                                    
                                var driver = new Drivers
                                {
                                    UserID = user.Id,
                                    SponsorID = model.SponsorID.Value,
                                    TotalPoints = 0
                                };
                                await _context.Drivers.AddAsync(driver);
                                break;

                            default:
                                throw new ArgumentException("Invalid user type");
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        Log.Information("Created new {UserType}: {Username}", model.UserType, user.UserName);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = $"{model.UserType} created successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create {UserType}", model.UserType);
                return StatusCode(500, new { 
                    error = "Failed to create user",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("delete-user")]

        [Authorize(Roles = "Admin")]
        [HttpGet("reports")] //separate into smaller async tasks...obv

        [HttpGet("system-stat-logs")] //Sprint 9, utilize a terminal console UI React component... :)



        [HttpGet("about")]
        public async Task<IActionResult> GetAbout()
        {
            var aboutInfo = await _context.About
            .OrderByDescending(a => a.Release)
            .LastOrDefaultAsync();
            if (aboutInfo == null)
            {
                return NotFound(new { message = "No about information found" });
            }

            return Ok(aboutInfo);
        }

        [HttpGet("test-db-connection")]
        public async Task<IActionResult> TestDbConnection()
        {
            try
            {
                var userCount = await _userManager.Users.CountAsync();
                return Ok($"Connection successful. User count: {userCount}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }

        //Method to whenever Admin needs to change or reset a user's password to random jumble
        //Temp password...
        private static string GeneratePassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public record CreateUserDto
    {
        //For all users; Required
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required UserType UserType { get; set; }
        public required string Password { get; set; }

        // For sponsors; So make them conditional
        public string? CompanyName { get; set; }
        public string? SponsorType { get; set; }
        public decimal? PointDollarValue { get; set; }

        // For drivers; So make them conditional
        public int? SponsorID { get; set; }

        public IEnumerable<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required");

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email is required");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required");

            switch (UserType)
            {
                case UserType.Sponsor when string.IsNullOrWhiteSpace(CompanyName):
                    errors.Add("Company name is required for sponsors");
                    break;
                case UserType.Sponsor when string.IsNullOrWhiteSpace(SponsorType):
                    errors.Add("Sponsor type is required for sponsors");
                    break;
                case UserType.Driver when !SponsorID.HasValue:
                    errors.Add("Sponsor ID is required for drivers");
                    break;
            }

            return errors;
        }
    };

}