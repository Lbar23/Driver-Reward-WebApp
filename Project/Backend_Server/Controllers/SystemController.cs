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
                                  IConfiguration configuration,
                                  NotifyService notifyService) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly AppDBContext _context = context;
        private readonly IConfiguration _configuration = configuration ;

        private readonly NotifyService _notifyService = notifyService;

        /********* API CALLS *********/

        /*** AUTH ***/
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            var user = new Users
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                UserType = UserType.Guest.ToString(),
                CreatedAt = DateTime.UtcNow,
                NotifyPref = NotificationPref.Email,
                EmailConfirmed = true, // development purposes, switch later
                PhoneNumberConfirmed = false,
                LockoutEnabled = false,
                TwoFactorEnabled = userDto.Enable2FA, // Set 2FA based on user's choice during registration
                AccessFailedCount = 0
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.UserType);
                Log.Information("UserID: {UserID}, Category: User, Description: User registered correctly! {Username}, {Id}",user.Id, user.UserName, user.Id);

                //New check to include new Sponsor details DTO; will add later...
                if (user.UserType == UserType.Sponsor.ToString())
                {
                    var sponsor = await _context.Sponsors.FirstOrDefaultAsync(s => s.UserID == user.Id);
                    if (sponsor != null)
                    {
                        var sponsorUser = new SponsorUsers
                        {
                            User = user,
                            Sponsor = sponsor,
                            IsPrimarySponsor = true,
                            JoinDate = DateTime.UtcNow,
                            SponsorRole = SponsorRole.Admin
                        };
                        await _context.SponsorUsers.AddAsync(sponsorUser);
                        await _context.SaveChangesAsync();
                    }
                }

                // Optionally, if 2FA is enabled, send the first 2FA code
                if (userDto.Enable2FA)
                {
                    var send2FaResult = await Send2FA(user);
                    if (!send2FaResult)
                    {
                        return StatusCode(500, new { message = "Failed to send 2FA code. Please try again later." });
                    }
                }

                return Ok(new { message = "User registered successfully", userId = user.Id, requiresTwoFactor = user.TwoFactorEnabled });
                }
                Log.Error("UserID: N/A, Category: User, Description: User registration failed");
                return BadRequest(result.Errors);
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] TwoFactorDto twoFactorDto)
        {
            var user = await _userManager.FindByIdAsync(twoFactorDto.UserId);
            if (user == null)
            {
                return Unauthorized(new { succeeded = false, message = "Invalid user", userId = twoFactorDto.UserId });
            }
            // Log the security stamp used during verification
            var securityStamp = await _userManager.GetSecurityStampAsync(user);
            Console.WriteLine($"Security Stamp: {securityStamp}");

            // Log the provider being used for the 2FA verification
            var provider = user.NotifyPref.ToString();
            Console.WriteLine($"Using Provider: {provider}");

            // Log the entered 2FA code
            Console.WriteLine($"Entered 2FA Code: {twoFactorDto.Code}");
            Console.WriteLine($"Token generated at (UTC): {DateTime.UtcNow}");
            var result = await _signInManager.TwoFactorSignInAsync("Email", twoFactorDto.Code, false, false);
            if (result.Succeeded)
            {
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new { succeeded = true, userId = user.Id });
            }

            // 2FA failed
            return Unauthorized(new { succeeded = false, message = "2FA failed", userId = twoFactorDto.UserId });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var user = await _userManager.FindByNameAsync(userDto.Username);
            if (user == null)
            {
                return Unauthorized(new { succeeded = false, message = "Invalid username or password" });
            }

            var result = await _signInManager.PasswordSignInAsync(userDto.Username, userDto.Password, false, false);

            if (result.Succeeded)
            {
                // Check if user has 2FA enabled
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    // Send 2FA code via preferred method (email or phone)
                    var send2FaResult = await Send2FA(user);
                    if (!send2FaResult)
                    {
                        Log.Error("UserID: {UserID}, Category: System, Description: Failed to send 2FA code to {User}",user.Id, user.UserName);
                        return StatusCode(500, new { succeeded = false, message = "Failed to send 2FA code. Please try again later." });
                    }

                    // 2FA is required
                    return Ok(new { succeeded = false, requiresTwoFactor = true, userId = user.Id });
                }

                // Login successful without 2FA
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Example from login method:
                Log.Information(
                    "UserID: {UserID}, Category: {Category}, Description: {Description}",
                    user.Id,
                    AuditLogCategory.User,
                    $"User {user.UserName} logged in successfully"
                );

                //New Response check if Sponsor User
                if (user.UserType == UserType.Sponsor.ToString())
                {
                    // Retrieve the sponsor user details
                    var sponsorUser = await _context.SponsorUsers
                        .Include(su => su.Sponsor)
                        .FirstOrDefaultAsync(su => su.UserID == user.Id);

                    if (sponsorUser != null)
                    {
                        return Ok(new
                        {
                            message = "Login successful",
                            userId = user.Id,
                            role = user.UserType,
                            succeeded = true,
                            sponsorDetails = new
                            {
                                sponsorId = sponsorUser.SponsorID,
                                companyName = sponsorUser.Sponsor.CompanyName,
                                isPrimarySponsor = sponsorUser.IsPrimarySponsor,
                                joinDate = sponsorUser.JoinDate,
                                sponsorRole = sponsorUser.SponsorRole
                            }
                        });
                    }
                }
                return Ok(new { message = "Login successful", userId = user.Id, role = user.UserType, succeeded = true });
            }

            Log.Error("UserID: {UserID}, Category: User, Description: Login Failed for {UserName}",user?.Id, user?.UserName);
            return Unauthorized(new { succeeded = false, message = "Invalid username or password" });
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {

            //Get the user before signing out
            var user = await _userManager.GetUserAsync(User);

            //Identity manager signs out user
            await _signInManager.SignOutAsync();

            //Deletes all cookies first
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            if (user != null)
            {
                Log.Information("UserID: {UserID}, Category: User, Description: {Username} of type {User} has successfully logged out", user?.Id, user?.UserName, user?.UserType.ToString());
                await _context.SaveChangesAsync();
            }
            else
            {
                Log.Error("UserID: {UserID}, Category: User, Description: {Username} of type {User} has failed to log out correctly", user?.Id, user?.UserName, user?.UserType.ToString());
                return StatusCode(500, "Failed to log out user");
            }
            
            return Ok(new { message = "Logged out successfully", succeeded = true});
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

        /********* HELPER FUNCTIONS *********/

        private async Task<bool> Send2FA(Users user)
        {
            // Get the security stamp used in token generation
            var securityStamp = await _userManager.GetSecurityStampAsync(user);

            Console.WriteLine($"Security Stamp: {securityStamp}");

            // Log the user's notification preference (provider)
            var provider = user.NotifyPref.ToString();
            Console.WriteLine($"Provider: {provider}");

            var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
            Console.WriteLine($"Generated 2FA Token: {code}");
            Console.WriteLine($"Token generated at (UTC): {DateTime.UtcNow}");


            switch (user.NotifyPref)
            {
                case NotificationPref.Phone:
                    if (!string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        await _notifyService.SendSmsAsync(user.PhoneNumber, $"Your 2FA code is: {code}");
                        return true;
                    }
                    break;

                case NotificationPref.Email:
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        var tData = new Dictionary<string, string> { { "auth_code", code } };
                        await _notifyService.SendTemplateEmail(user.Email, "2FA", tData);
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
