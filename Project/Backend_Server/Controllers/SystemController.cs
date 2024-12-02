using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Backend_Server.Services;
using Serilog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using static Backend_Server.Services.ClaimsService;
using Renci.SshNet;


namespace Backend_Server.Controllers
{
    /// <summary>
    /// SystemController:
    /// 
    /// This controller manages system actions, including registration, login, 2FA handling,
    /// notifications, and dynamic pages.
    ///
    /// Endpoints:
    /// 
    /// [POST]  /api/system/register                - Registers a new user
    /// [POST]  /api/system/verify-2fa              - Verifies a user's 2FA code
    /// [POST]  /api/system/login                   - Logs in a user
    /// [POST]  /api/system/logout                  - Logs out the current user
    /// [GET]   /api/system/about                   - Retrieves latest "About" information
    /// [GET]   /api/system/test-db-connection      - Tests the database connection
    /// 
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController(UserManager<Users> userManager, 
                                  SignInManager<Users> signInManager, 
                                  AppDBContext context, 
                                  IMemoryCache cache,
                                  IConfiguration configuration,
                                  NotifyService notifyService,
                                  ClaimsService claimsService) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly AppDBContext _context = context;
        private readonly IMemoryCache _cache = cache;
        private readonly IConfiguration _configuration = configuration;
        private readonly NotifyService _notifyService = notifyService;
        private readonly ClaimsService _claimsService = claimsService;

        /********* API CALLS *********/

        /*** AUTH ***/
        [HttpPost("register")]
        public async Task<IActionResult> Register(CreateUserDto userDto)
        {
            var user = new Users
            {
                UserName = userDto.Username,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                CreatedAt = DateTime.UtcNow,
                NotifyPref = NotificationPref.Email,
                State = userDto.State,
                EmailConfirmed = true,
                TwoFactorEnabled = userDto.Enable2FA,
                RoleID = 4
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (!result.Succeeded)
            {
                Log.Error("User registration failed: {Errors}", result.Errors);
                return BadRequest(result.Errors);
            }

            // Assign role
            var roleResult = await _userManager.AddToRoleAsync(user, userDto.Role);
            if (!roleResult.Succeeded)
            {
                Log.Error("Failed to assign role {Role} to user {UserId}: {Errors}", userDto.Role, user.Id, roleResult.Errors);
                return BadRequest(roleResult.Errors);
            }

            Log.Information("User registered successfully: {Username} ({UserId})", user.UserName, user.Id);

            // Optionally send 2FA
            if (userDto.Enable2FA)
            {
                var send2FaResult = await Send2FA(user);
                if (!send2FaResult)
                {
                    return StatusCode(500, new { message = "Failed to send 2FA code. Please try again later." });
                }
            }

            var token = await _claimsService.GenerateJwtToken(user);
            SetJwtCookie(token);
            
            return Ok(new
            {
                message = "User registered successfully",
                userId = user.Id,
                requiresTwoFactor = user.TwoFactorEnabled
            });
        }

        [HttpPost("login")]
public async Task<IActionResult> Login(UserLoginDto userDto)
{
    var user = await _userManager.FindByNameAsync(userDto.Username);
    if (user == null)
    {
        return Unauthorized(new { succeeded = false, message = "Invalid username or password" });
    }

    // First sign out any existing session
    await _signInManager.SignOutAsync();
    
    // Attempt password sign in
    var result = await _signInManager.PasswordSignInAsync(user, userDto.Password, false, false);
    
    if (!result.Succeeded && !result.RequiresTwoFactor)
    {
        Log.Error("Login failed for user {Username}", userDto.Username);
        return Unauthorized(new { message = "Invalid username or password" });
    }

    if (user.TwoFactorEnabled)
    {
        // Generate and send 2FA token
        var send2FaResult = await Send2FA(user);
        if (!send2FaResult)
        {
            Log.Error($"Failed to send 2FA code for user {user.Id}");
            return StatusCode(500, "Failed to send 2FA code");
        }

        return Ok(new { 
            succeeded = false, 
            requiresTwoFactor = true, 
            userId = user.Id 
        });
    }

    var token = await _claimsService.GenerateJwtToken(user);
    SetJwtCookie(token);

    user.LastLogin = DateTime.UtcNow;
    await _userManager.UpdateAsync(user);

    return Ok(new { message = "Login successful", userId = user.Id });
}

[HttpPost("verify-2fa")]
public async Task<IActionResult> Verify2FA([FromBody] TwoFactorDto twoFactorDto)
{
    try 
    {
        Log.Information("2FA verification attempt for userId: {UserId} with code: {Code}", 
            twoFactorDto.UserId, twoFactorDto.Code);

        var user = await _userManager.FindByIdAsync(twoFactorDto.UserId);
        if (user == null)
        {
            Log.Warning("User not found for 2FA verification: {UserId}", twoFactorDto.UserId);
            return Unauthorized(new { succeeded = false, message = "Invalid user" });
        }

        // Generate a new token and compare with the provided code
        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        Log.Information("Generated token for comparison: {Token}", token);

        if (token != twoFactorDto.Code.Trim())
        {
            Log.Warning("Invalid 2FA code provided for user {UserId}", twoFactorDto.UserId);
            return Unauthorized(new { 
                succeeded = false, 
                message = "Invalid verification code" 
            });
        }

        // Sign in the user
        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(user, false);

        // Generate and set JWT token
        var jwtToken = await _claimsService.GenerateJwtToken(user);
        SetJwtCookie(jwtToken);

        // Update last login
        user.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        Log.Information("2FA verification successful for user {UserId}", user.Id);

        return Ok(new { 
            succeeded = true, 
            userId = user.Id,
            message = "2FA verification successful"
        });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error during 2FA verification for userId: {UserId}", twoFactorDto.UserId);
        return StatusCode(500, new { 
            succeeded = false, 
            message = "An error occurred during verification" 
        });
    }
}

private async Task<bool> Send2FA(Users user)
{
    try
    {
        // Generate the 2FA token
        var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
        if (string.IsNullOrEmpty(token))
        {
            Log.Error($"Failed to generate 2FA token for user {user.Id}");
            return false;
        }

        Log.Information("Generated 2FA token for user {UserId}: {Token}", user.Id, token);

        // Send the notification
        await _notifyService.NotifyAuthAsync(user.Id, token, user.UserName ?? "User");
        return true;
    }
    catch(Exception ex)
    {
        Log.Error(ex, "2FA notify attempt failed for user {UserId}", user.Id);
        return false;
    }
}



        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            await _claimsService.RemoveImpersonation(user);
            await _signInManager.SignOutAsync();
            RemoveJwtCookie();

            Log.Information("User {UserId} logged out", user.Id);
            return Ok(new { message = "Logged out successfully" });
        }


        /*** DYNAMIC PAGE ***/
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

        [Authorize(Policy = ClaimsService.CustomClaimTypes.SystemPreferences)]
        [HttpPost("impersonate/{targetUserId}")]
        public async Task<IActionResult> StartImpersonation(int targetUserId)
        {
            try
            {
                var impersonator = await _userManager.GetUserAsync(User);
                var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());

                if (impersonator == null || targetUser == null)
                    return NotFound("User not found");

                // Get claims for impersonation
                var claims = await _claimsService.SetImpersonation(impersonator, targetUser);
                
                // Generate new token with impersonation claims
                var token = await _claimsService.GenerateJwtToken(targetUser, claims);

                // Set the token in a cookie
                Response.Cookies.Append("X-Access-Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                Log.Information(
                    "User {ImpersonatorId} started impersonating user {TargetUserId}", 
                    impersonator.Id, targetUserId);

                return Ok(new { message = "Impersonation started" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error starting impersonation");
                return StatusCode(500, "Failed to start impersonation");
            }
        }

        [AllowAnonymous]
        [HttpGet("generate-hash")]
        public IActionResult GenerateHash()
        {
            var hasher = new PasswordHasher<Users>();
            var hash = hasher.HashPassword(null, "Admin@123!");
            Log.Information(hash);
            return Ok(new { hash });
        }

        [HttpPost("impersonate/stop")]
        public async Task<IActionResult> StopImpersonation()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return NotFound("User not found");

                // Get the original user ID from claims
                var originalUserId = User.FindFirst(ClaimsService.CustomClaimTypes.OriginalUserId)?.Value;
                if (string.IsNullOrEmpty(originalUserId))
                    return BadRequest("No active impersonation");

                var originalUser = await _userManager.FindByIdAsync(originalUserId);
                if (originalUser == null)
                    return NotFound("Original user not found");

                // Remove impersonation and get original user claims
                await _claimsService.RemoveImpersonation(currentUser);
                var claims = await _claimsService.GetUserClaims(originalUser);
                
                // Generate new token with original user claims
                var token = await _claimsService.GenerateJwtToken(originalUser, claims);

                Response.Cookies.Append("X-Access-Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                Log.Information(
                    "User {UserId} stopped impersonation", 
                    originalUser.Id);

                return Ok(new { message = "Impersonation stopped" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping impersonation");
                return StatusCode(500, "Failed to stop impersonation");
            }
        }
        /********* HELPER FUNCTIONS *********/

        // private async Task<bool> Send2FA(Users user)
        // {
        //     try{
        //         var code = await _userManager.GenerateTwoFactorTokenAsync(user, user.NotifyPref.ToString());
        //         await _notifyService.NotifyAuthAsync(user.Id, code, user.UserName ?? "User");
        //         return true;
        //     }
        //     catch(Exception ex){
        //         Log.Error($"2FA notify attempt failed: {ex}");
        //         return false;
        //     }
            
        // }


        private void SetJwtCookie(string token)
        {
            Response.Cookies.Append("X-Access-Token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            });
        }

        private void RemoveJwtCookie()
        {
            Response.Cookies.Delete("X-Access-Token");
        }


    }
}
