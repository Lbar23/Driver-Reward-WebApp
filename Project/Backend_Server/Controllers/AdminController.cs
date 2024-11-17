using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Backend_Server.Models;
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
    /// This controller provides endpoints for managing users, including creation, deletion,
    /// role updates, and retrieval of details and audit logs.
    /// 
    /// Endpoints:
    /// 
    /// [POST] /api/admin/create-user       - Creates a new user (Admin, Sponsor, Driver)
    /// [POST] /api/admin/change-user-type  - Changes the user type for an existing user
    /// [DELETE] /api/admin/remove-user     - Removes a user and associated records
    /// [GET] /api/admin/reports            - Fetches reports (placeholder)
    /// [GET] /api/admin/system-stat-logs   - Fetches system stats and logs (placeholder)
    /// [GET] /api/admin/about              - Retrieves latest "About" information
    /// [GET] /api/admin/test-db-connection - Tests the database connection
    /// [GET] /api/admin/drivers/details    - Retrieves driver details with sponsor relationships
    /// [GET] /api/admin/sponsors/details   - Retrieves sponsor details
    /// [GET] /api/admin/admins/details     - Retrieves admin details
    /// [GET] /api/admin/audit-logs         - Fetches audit logs with filters and pagination
    /// [GET] /api/admin/audit-logs/export  - Exports audit logs as a CSV
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController(AppDBContext context, UserManager<Users> userManager, NotifyService notifyService, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly AppDBContext _context = context;
        private readonly UserManager<Users> _userManager = userManager;
        private readonly NotifyService _notifyService = notifyService;

        /// <summary>
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


        [Authorize(Roles = "Admin")]
        [HttpGet("reports")] //separate into smaller async tasks...obv

        [HttpGet("system-stat-logs")] //Sprint 9, utilize a terminal console UI React component... :)


        [AllowAnonymous]
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

        //read audit logs from database
        [HttpGet("audit-logs")]
        [Authorize(Roles = "Admin, Sponsor")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int? userId = null,
            [FromQuery] string? category = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            return await ExecuteQueryWithRetryAsync<IActionResult>(async () =>
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                string cacheKey = $"audit_logs_{currentUser.Id}_{userId}_{category}_{startDate}_{endDate}_{page}_{pageSize}";

                return await GetCachedAsync(cacheKey, async () =>
                {
                    try
                    {
                        var query = _context.AuditLogs.AsQueryable();

                        // If sponsor, restrict to their drivers' logs
                        if (currentUser.UserType == UserType.Sponsor.ToString())
                        {
                            var sponsorId = await _context.Sponsors
                                .Where(s => s.UserID == currentUser.Id)
                                .Select(s => s.SponsorID)
                                .FirstOrDefaultAsync();

                            var sponsorDriverIds = await _context.SponsorDrivers
                                .Where(sd => sd.SponsorID == sponsorId)
                                .Select(sd => sd.DriverID)
                                .ToListAsync();

                            query = query.Where(l => sponsorDriverIds.Contains(l.UserID));
                        }

                        // Apply filters
                        if (userId.HasValue)
                            query = query.Where(l => l.UserID == userId);

                        if (!string.IsNullOrEmpty(category) && Enum.TryParse(category, out AuditLogCategory categoryEnum))
                            query = query.Where(l => l.Category == categoryEnum);

                        if (startDate.HasValue)
                            query = query.Where(l => l.Timestamp >= startDate.Value);

                        if (endDate.HasValue)
                            query = query.Where(l => l.Timestamp <= endDate.Value);

                        var totalCount = await query.CountAsync();

                        var logs = await query
                            .OrderByDescending(l => l.Timestamp)
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(l => new
                            {
                                l.LogID,
                                l.Timestamp,
                                l.Category,
                                l.UserID,
                                UserName = _context.Users
                                    .Where(u => u.Id == l.UserID)
                                    .Select(u => u.UserName)
                                    .FirstOrDefault(),
                                l.Description
                            })
                            .ToListAsync();

                        return Ok(new
                        {
                            totalCount,
                            page,
                            pageSize,
                            logs
                        });
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error retrieving audit logs");
                        return StatusCode(500, "Error retrieving audit logs");
                    }
                }, TimeSpan.FromMinutes(5));
            });
        }

        //aduit log CSV export
        [HttpGet("audit-logs/export")]
        [Authorize(Roles = "Admin, Sponsor")]
        public async Task<IActionResult> ExportAuditLogs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? category)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            try
            {
                var query = _context.AuditLogs.AsQueryable();

                // Apply the same permission filtering as above
                if (currentUser.UserType == UserType.Sponsor.ToString())
                {
                    var sponsorId = await _context.Sponsors
                        .Where(s => s.UserID == currentUser.Id)
                        .Select(s => s.SponsorID)
                        .FirstOrDefaultAsync();

                    var sponsorDriverIds = await _context.SponsorDrivers
                        .Where(sd => sd.SponsorID == sponsorId)
                        .Select(sd => sd.DriverID)
                        .ToListAsync();

                    query = query.Where(l => sponsorDriverIds.Contains(l.UserID));
                }

                // Apply date range filter
                if (startDate.HasValue)
                    query = query.Where(l => l.Timestamp >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(l => l.Timestamp <= endDate.Value);

                // Apply category filter
                if (!string.IsNullOrEmpty(category) && Enum.TryParse<AuditLogCategory>(category, out var categoryEnum))
                    query = query.Where(l => l.Category == categoryEnum);

                var logs = await query
                    .OrderByDescending(l => l.Timestamp)
                    .Select(l => new
                    {
                        l.LogID,
                        Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        l.Category,
                        l.UserID,
                        l.Description,
                        UserName = _context.Users
                            .Where(u => u.Id == l.UserID)
                            .Select(u => u.UserName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                // Create CSV content
                var csv = new StringBuilder();
                csv.AppendLine("Log ID,Timestamp,Category,User ID,User Name,Description");
                
                foreach (var log in logs)
                {
                    csv.AppendLine($"{log.LogID},{log.Timestamp},{log.Category},{log.UserID},{log.UserName},{log.Description}");
                }

                return File(Encoding.UTF8.GetBytes(csv.ToString()), 
                    "text/csv", 
                    $"audit_logs_{DateTime.Now:yyyyMMdd}.csv");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UserID: {UserID}, Category: {Category}, Description: Error exporting audit logs",
                    currentUser.Id,
                    AuditLogCategory.System);
                return StatusCode(500, "Error exporting audit logs");
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
    public class ChangeUserTypeDto
    {
        public int UserId { get; set; }
        public string NewUserType { get; set; }
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