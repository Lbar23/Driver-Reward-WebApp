using Microsoft.AspNetCore.Mvc;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Backend_Server.Services;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Backend_Server.Infrastructure;
using System.Security.Claims;
using static Backend_Server.Services.ClaimsService;

namespace Backend_Server.Controllers
{
    /// <summary>
    /// AdminController:
    /// 
    /// This controller handles administrative functions related to user management,
    /// including user creation, modification, and role management. It provides
    /// functionality for both admin and sponsor users to manage their respective
    /// user hierarchies.
    /// 
    /// Endpoints:
    /// 
    /// [POST]   /api/admin/create-user             - Creates new users (Admin, Sponsor, Driver, Guest
    /// [POST]   /api/admin/change-user-type        - Changes existing user's role/type, updates associated role data
    /// [DELETE] /api/admin/remove-user/{userId}    - Removes user and all associated data
    /// [GET]    /api/admin/view-users/{userType}   - Retrieves users of specified type
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : CachedBaseController
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly NotifyService _notifyService;
        private readonly ClaimsService _claimsService;

        public AdminController(
            AppDBContext context,
            UserManager<Users> userManager,
            NotifyService notifyService,
            ClaimsService claimsService,
            IMemoryCache cache) : base(cache)
        {
            _context = context;
            _userManager = userManager;
            _notifyService = notifyService;
            _claimsService = claimsService;
        }

        [Authorize(Roles = "Admin, Sponsor")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var user = await CreateBaseUser(model);
                        await AddUserTypeSpecificData(user, model.Role, model.SponsorID, model.IsPrimary);
                        
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        Log.Information("UserID: {UserID}, Category: User, Description: Created new {UserType}: {Username}", 
                            user.Id, model.Role, user.UserName);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = $"{model.Role} created successfully" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "UserID: N/A, Category: User, Description: Failed to create {Role}", model.Role);
                return StatusCode(500, new { error = "Failed to create user", details = ex.Message });
            }
        }

        [Authorize(Policy = PolicyNames.RequireAdminRole)]
        [HttpPost("change-user-type")]
        public async Task<IActionResult> ChangeUserType([FromBody] ChangeUserTypeDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var strategy = _context.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var user = await _userManager.FindByIdAsync(model.UserId.ToString());
                    if (user == null)
                        return new { Success = false, Message = "User not found" };

                    var oldUserType = user.Role?.Name ?? "Unknown";

                    await RemoveUserTypeData(user, oldUserType);
                    await UpdateUserRole(user, oldUserType, model.NewUserType);
                    await AddUserTypeSpecificData(user, model.NewUserType, model.Sponsor?.SponsorID, false);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (user.UserName != null)
                    {
                        await _notifyService.NotifySystemChangeAsync(user.Id, "RoleChange", user.UserName);
                    }

                    Log.Information("Changed user type for {Username} from {OldType} to {NewType}", 
                        user.UserName, oldUserType, model.NewUserType);

                    return new { Success = true, Message = "User type changed successfully" };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Log.Error(ex, "Failed to change user type for {UserId}", model.UserId);
                    throw;
                }
            });

            if (!result.Success)
                return NotFound(new { error = result.Message });

            return Ok(new { message = result.Message, userType = model.NewUserType });
        }

        [HttpDelete("remove-user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var user = await _userManager.FindByIdAsync(userId);
                        if (user == null)
                            throw new KeyNotFoundException("User not found");

                        var userType = user.Role?.Name ?? "Unknown";
                        
                        // Remove role-specific data
                        await RemoveUserTypeData(user, userType);
                        
                        // Remove claims and roles
                        var userClaims = await _userManager.GetClaimsAsync(user);
                        var userRoles = await _userManager.GetRolesAsync(user);
                        
                        if (userClaims.Any())
                            await _userManager.RemoveClaimsAsync(user, userClaims);
                        
                        if (userRoles.Any())
                            await _userManager.RemoveFromRolesAsync(user, userRoles);

                        // Delete the user
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            throw new InvalidOperationException(
                                $"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        Log.Information("UserID: {UserId}, Category: User, Description: User {UserType} removed successfully", 
                            userId, userType);
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok(new { message = "User removed successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                Log.Warning(ex, "Attempted to remove non-existent user {UserId}", userId);
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, "Failed to remove user {UserId}", userId);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Critical error removing user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to remove user", details = ex.Message });
            }
        }

        
        [HttpGet("view-users/{userType}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersDetails(string userType)
        {
            try
            {
                IEnumerable<object> details = userType.ToLower() switch
                {
                    "drivers" => await _context.Set<ViewDriversDto>()
                        .FromSqlRaw("SELECT * FROM v_AllDrivers")
                        .AsNoTracking()
                        .ToListAsync() ?? new List<ViewDriversDto>(),
                        
                    "sponsors" => await _context.Set<ViewSponsorUsersDto>()
                        .FromSqlRaw("SELECT * FROM v_AllSponsorUsers")
                        .AsNoTracking()
                        .ToListAsync() ?? new List<ViewSponsorUsersDto>(),
                        
                    "admins" => await _context.Set<ViewAdminsDto>()
                        .FromSqlRaw("SELECT * FROM v_AllAdmins")
                        .AsNoTracking()
                        .ToListAsync() ?? new List<ViewAdminsDto>(),
                        
                    _ => throw new ArgumentException($"Invalid user type: {userType}")
                };

                return Ok(details);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to retrieve {UserType} details", userType);
                return StatusCode(500, $"Failed to retrieve {userType} details");
            }
        }

        // Consolidated helper methods
        private async Task<Users> CreateBaseUser(CreateUserDto model)
        {
            var user = new Users
            {
                UserName = model.Username,
                FirstName = model.FirstName,
                LastName = model.Username,
                Email = model.Email,
                State = model.State,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var password = !string.IsNullOrEmpty(model.Password) ? model.Password : GeneratePassword();
            var result = await _userManager.CreateAsync(user, password);
            
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, model.Role);
            await _userManager.AddClaimsAsync(user, new[] { new Claim(ClaimTypes.Role, model.Role) });

            return user;
        }

        private async Task RemoveUserTypeData(Users user, string userType)
        {
            switch (userType.ToLower())
            {
                case "sponsor":
                    var sponsorUser = await _context.SponsorUsers
                        .Include(su => su.Sponsor)
                        .FirstOrDefaultAsync(su => su.UserID == user.Id);
                    if (sponsorUser != null)
                    {
                        _context.SponsorUsers.Remove(sponsorUser);
                        _context.Sponsors.Remove(sponsorUser.Sponsor);
                    }
                    break;

                case "driver":
                    var driver = await _context.SponsorDrivers
                        .FirstOrDefaultAsync(d => d.UserID == user.Id);
                    if (driver != null)
                    {
                        _context.SponsorDrivers.Remove(driver);
                    }
                    break;
            }
        }

        private async Task AddUserTypeSpecificData(Users user, string userType, int? sponsorId, bool? isPrimary)
        {
            if (sponsorId.HasValue)
            {
                var sponsor = await _context.Sponsors.FirstOrDefaultAsync(s => s.SponsorID == sponsorId.Value)
                    ?? throw new InvalidOperationException("Sponsor not found.");

                switch (userType.ToLower())
                {
                    case "sponsor":
                        var sponsorUser = new SponsorUsers
                        {
                            UserID = user.Id,
                            User = user,
                            Sponsor = sponsor,
                            SponsorID = sponsorId.Value,
                            IsPrimary = isPrimary ?? false,
                            JoinDate = DateTime.UtcNow
                        };
                        await _context.SponsorUsers.AddAsync(sponsorUser);
                        break;

                    case "driver":
                        var sponsorDriver = new SponsorDrivers
                        {
                            UserID = user.Id,
                            SponsorID = sponsor.SponsorID,
                            User = user,
                            Sponsor = sponsor,
                            Points = 0,
                            DriverPointValue = sponsor.PointDollarValue,
                            MilestoneLevel = sponsor.MilestoneThreshold == 0 ? 0 : 1,
                        };
                        await _context.SponsorDrivers.AddAsync(sponsorDriver);
                        break;
                }
            }
        }

        private async Task UpdateUserRole(Users user, string oldRole, string newRole)
        {
            if (!string.IsNullOrEmpty(oldRole))
            {
                await _userManager.RemoveFromRoleAsync(user, oldRole);
                var oldClaims = await _userManager.GetClaimsAsync(user);
                await _userManager.RemoveClaimsAsync(user, oldClaims);
            }

            await _userManager.AddToRoleAsync(user, newRole);
            await _userManager.AddClaimsAsync(user, new[] { new Claim(ClaimTypes.Role, newRole) });
            await _claimsService.UpdateUserClaims(user);
        }

        private static string GeneratePassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[Random.Shared.Next(s.Length)])
                .ToArray());
        }
    }
}