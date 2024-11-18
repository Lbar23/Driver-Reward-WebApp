using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend_Server.Services;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Backend_Server.Infrastructure;

namespace Backend_Server.Controllers
{
    /// <summary>
    /// AdminController:
    /// 
    /// This controller provides endpoints for managing users, including creation, deletion,
    /// role updates, and retrieval of details and audit logs.
    /// 
    /// Endpoints:
    /// 
    /// [POST]      /api/admin/create-user          - Creates a new user (Admin, Sponsor, Driver)
    /// [POST]      /api/admin/change-user-type     - Changes the user type for an existing user
    /// [DELETE]    /api/admin/remove-user          - Removes a user and associated records
    /// [GET]       /api/admin/drivers/details      - Retrieves driver details with sponsor relationships
    /// [GET]       /api/admin/sponsors/details     - Retrieves sponsor details
    /// [GET]       /api/admin/admins/details       - Retrieves admin details
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController(AppDBContext context, UserManager<Users> userManager, NotifyService notifyService, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly AppDBContext _context = context;
        private readonly UserManager<Users> _userManager = userManager;
        private readonly NotifyService _notifyService = notifyService;

        /********* API CALLS *********/

        /// <summary>
        /// -- Admin creates any Users (Admin, Driver, Sponsors) -- Guests aren't needed since they register normally
        /// -- Sponsors can also create other Sponsors (under their Company Name) and Drivers (Also under their Company Name)
        /// -- Database current has 3 Sponsors (to test insertion) with same password hash; Delete them later to actually add in rememberable passwords
        /// -- Current implementation can be simplified, but this was done in like, 2 hours of API testing. There's no frontend yet; waiting for updates...
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Sponsor")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            // Get the execution strategy from the context
            var strategy = _context.Database.CreateExecutionStrategy();

            try{
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
                                    // SponsorID = model.SponsorID.Value,
                                    // TotalPoints = 0
                                };
                                await _context.Drivers.AddAsync(driver);
                                break;

                            default:
                                throw new ArgumentException("Invalid user type");
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        Log.Information("UserID: {UserID}, Category: User, Description: Created new {UserType}: {Username}", user.Id, model.UserType, user.UserName);
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
                Log.Error(ex, "UserID: N/A, Category: User, Description: Failed to create {UserType}", model.UserType);
                return StatusCode(500, new { 
                    error = "Failed to create user",
                    details = ex.Message
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("change-user-type")]
        public async Task<IActionResult> ChangeUserType([FromBody] ChangeUserTypeDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                        if (user == null)
                            throw new InvalidOperationException("User not found");

                        // Remove from old role
                        var currentRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);

                        // Add to new role
                        await _userManager.AddToRoleAsync(user, model.NewUserType);

                        // Update user type in Users table
                        user.UserType = model.NewUserType;
                        await _userManager.UpdateAsync(user);

                        // Handle type-specific records
                        // Remove old type-specific record
                        switch (user.UserType)
                        {
                            case "Admin":
                                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UserID == user.Id);
                                if (admin != null)
                                    _context.Admins.Remove(admin);
                                break;
                            case "Sponsor":
                                var sponsor = await _context.Sponsors.FirstOrDefaultAsync(s => s.UserID == user.Id);
                                if (sponsor != null)
                                    _context.Sponsors.Remove(sponsor);
                                break;
                            case "Driver":
                                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserID == user.Id);
                                if (driver != null)
                                    _context.Drivers.Remove(driver);
                                break;
                        }

                        // Add new type-specific record
                        switch (model.NewUserType)
                        {
                            case "Admin":
                                await _context.Admins.AddAsync(new Admins { UserID = user.Id });
                                break;
                            case "Sponsor":
                                await _context.Sponsors.AddAsync(new Sponsors 
                                { 
                                    UserID = user.Id,
                                    CompanyName = "Pending Update", // Default value, should be updated later
                                    SponsorType = "Standard",
                                    PointDollarValue = 0.01m
                                });
                                break;
                            case "Driver":
                                await _context.Drivers.AddAsync(new Drivers 
                                { 
                                    UserID = user.Id
                                });
                                break;
                            default:
                                throw new ArgumentException("Invalid user type");
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        Log.Information("Changed user type for {Username} to {NewUserType}", user.UserName, model.NewUserType);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = "User type changed successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to change user type for user {UserId}", model.UserId);
                return StatusCode(500, new { 
                    error = "Failed to change user type",
                    details = ex.Message 
                });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("remove-user/{userId}")]
        public async Task<IActionResult> RemoveUser(int userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var user = await _userManager.FindByIdAsync(userId.ToString());
                        if (user == null)
                            throw new InvalidOperationException("User not found");

                        // Store email for notification before deletion
                        var userEmail = user.Email;
                        var userName = user.UserName;
                        var userType = user.UserType;

                        // Remove type-specific record first
                        switch (userType)
                        {
                            case "Admin":
                                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UserID == userId);
                                if (admin != null)
                                    _context.Admins.Remove(admin);
                                break;
                            case "Sponsor":
                                var sponsor = await _context.Sponsors.FirstOrDefaultAsync(s => s.UserID == userId);
                                if (sponsor != null)
                                    _context.Sponsors.Remove(sponsor);
                                break;
                            case "Driver":
                                var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.UserID == userId);
                                if (driver != null)
                                    _context.Drivers.Remove(driver);
                                break;
                        }

                        // Remove user from roles
                        var userRoles = await _userManager.GetRolesAsync(user);
                        await _userManager.RemoveFromRolesAsync(user, userRoles);

                        // Delete the user
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                            throw new InvalidOperationException(
                                string.Join(", ", result.Errors.Select(e => e.Description)));

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();


                        Log.Information("Removed user: {Username} ({UserType})", userName, userType);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = "User removed successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to remove user {UserId}", userId);
                return StatusCode(500, new { 
                    error = "Failed to remove user",
                    details = ex.Message 
                });
            }
        }

        [HttpGet("drivers/details")]
        [Authorize(Roles = "Admin, Sponsor")]
        public async Task<IActionResult> GetDriversWithDetails()
        {
            return await ExecuteQueryWithRetryAsync(async () =>
            {
                var currentUser = await _userManager.GetUserAsync(User);
                string cacheKey = $"drivers_details_{currentUser?.Id}";
                
                return await GetCachedAsync(cacheKey, async () =>
                {
                    try
                    {
                        var driversWithDetails = await _context.Users
                            .Where(u => u.UserType == "Driver")
                            .Select(u => new
                            {
                                userId = u.Id,
                                name = u.UserName,
                                email = u.Email,
                                sponsorRelationships = _context.SponsorDrivers
                                    .Where(sd => sd.DriverID == u.Id)
                                    .Select(sd => new
                                    {
                                        sponsorId = sd.SponsorID,
                                        sponsorName = _context.Sponsors
                                            .Where(s => s.SponsorID == sd.SponsorID)
                                            .Select(s => s.CompanyName)
                                            .FirstOrDefault(),
                                        points = sd.Points
                                    })
                                    .ToList()
                            })
                            .ToListAsync();

                        return Ok(driversWithDetails);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error fetching drivers with details");
                        return StatusCode(500, "Error retrieving driver details");
                    }
                }, TimeSpan.FromMinutes(15));
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("sponsors/details")]
        public async Task<IActionResult> GetSponsorsDetails()
        {
            try
            {
                var sponsorsQuery = from u in _context.Users
                                join s in _context.Sponsors on u.Id equals s.UserID
                                where u.UserType == "Sponsor"
                                select new
                                {
                                    UserId = u.Id,
                                    Name = u.UserName,
                                    Email = u.Email,
                                    SponsorID = s.SponsorID,
                                    CompanyName = s.CompanyName,
                                    SponsorType = s.SponsorType,
                                    PointDollarValue = s.PointDollarValue,
                                    //TotalDrivers = _context.Drivers.Count(d => d.SponsorID == s.SponsorID),
                                    UserType = u.UserType
                                };

                var sponsors = await sponsorsQuery.ToListAsync();
                return Ok(sponsors);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve sponsors details");
                return StatusCode(500, new { error = "Failed to retrieve sponsors details" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admins/details")]
        public async Task<IActionResult> GetAdminsDetails()
        {
            try
            {
                var adminsQuery = from u in _context.Users
                                join a in _context.Admins on u.Id equals a.UserID
                                where u.UserType == "Admin"
                                select new
                                {
                                    UserId = u.Id,
                                    Name = u.UserName,
                                    Email = u.Email,
                                    CreatedAt = u.CreatedAt,
                                    LastLogin = u.LastLogin,
                                    UserType = u.UserType
                                };

                var admins = await adminsQuery.ToListAsync();
                return Ok(admins);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve admins details");
                return StatusCode(500, new { error = "Failed to retrieve admins details" });
            }
        }

        /********* HELPER FUNCTIONS *********/

        // Method to whenever Admin needs to change or reset a user's password to random jumble
        // Temp password...
        private static string GeneratePassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

}