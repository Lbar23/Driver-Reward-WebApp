using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Microsoft.AspNetCore.Authorization;
using Backend_Server.Services;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<Users> userManager, SignInManager<Users> signInManager, AppDBContext context, NotifyService notifyService) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly AppDBContext _context = context;
        private readonly NotifyService _notifyService = notifyService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            var user = new Users
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                UserType = DetermineUserRole(userDto.RegistrationCode),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.UserType);
                return Ok(new { message = "User registered successfully", role = user.UserType });
            }

            return BadRequest(result.Errors);
        }

        // Separate function to send 2FA code
        private async Task<bool> Send2FA(Users user)
        {
            string code = await _userManager.GenerateTwoFactorTokenAsync(user, user.NotifyPref.ToString());

            switch (user.NotifyPref)
                {
                    case NotificationPref.Phone:
                        // Send via AWS SNS (SMS)
                        await _notifyService.SendSmsAsync(user.PhoneNumber, $"Your 2FA code is: {code}");
                        break;
                    case NotificationPref.Email:
                        // Send via AWS SES (Email)
                        await _notifyService.SendEmailAsync(user.Email, "Your 2FA Code", $"Your 2FA code is: {code}");
                        break;
                    default:
                        return false;
                }

            return true;
        }

        // Function to verify 2FA code
        [HttpPost("verify-2fa")]
        public async Task<IActionResult> Verify2FA([FromBody] TwoFactorDto twoFactorDto)
        {
            var user = await _userManager.FindByIdAsync(twoFactorDto.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid user" });
            }

            var result = await _signInManager.TwoFactorSignInAsync(user.NotifyPref.ToString(), twoFactorDto.Code, false, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "2FA failed" });
            }

            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "2FA successful", userId = user.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var result = await _signInManager.PasswordSignInAsync(userDto.Username, userDto.Password, false, false);
            var user = await _userManager.FindByNameAsync(userDto.Username);
            if (result.Succeeded && user != null)
            {
                // check for 2fa
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    var send2FaResult = await Send2FA(user);
                    if (!send2FaResult)
                    {
                        return StatusCode(500, new { message = "Failed to send 2FA code. Please try again later." });
                    }

                    return Ok(new { message = "2FA required", userId = user.Id });
                }
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new { message = "Login successful", userId = user.Id, role = user.UserType });
            }

            return Unauthorized("Invalid username or password");
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

        [Authorize] // has to be protected
        [HttpGet("currentuser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Gets the current logged-in user based on the HttpContext
            var user = await _userManager.GetUserAsync(User); 
            if (user == null)
            {
                return Unauthorized("User is not logged in");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissions(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.UserType,
                user.CreatedAt,
                user.LastLogin,
                Roles = roles,
                Permissions = permissions
            });
        }

        [HttpGet("getuser")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetUserPermissions(user);
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.UserType,
                user.CreatedAt,
                user.LastLogin,
                Roles = roles,
                Permissions = permissions
            });
            
        }

        //Permission task to grab the entire list of specific permissions for the specified user(s)
        private async Task<List<string>> GetUserPermissions(Users user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _context.Permissions // <-- Will throw error until I update database & AppDBContext
                .Where(p => roles.Contains(p.Role))
                .Select(p => p.PermissionName)
                .Distinct()
                .ToListAsync();
            return permissions;
        }
    // will be replaced with real reg code
        private static string DetermineUserRole(string registrationCode)
        {
            switch (registrationCode.ToUpper())
            {
                case "ADMIN2023":
                    return "Admin";
                case "SPONSOR2023":
                    return "Sponsor";
                default:
                    return "Driver";
            }
        }
    }

    public class UserRegisterDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string RegistrationCode { get; set; }
        //public string CompanyName { get; set; }
    }

    public class UserLoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class SponserAccessDto
    {
        public required string AccessCode { get; set; } // <-- Access code based on Sponsor (unique specific ones for different Sponsors, but the same code for the same Sponsors)
    }
}
public class TwoFactorDto
    {
        public required string UserId { get; set; }
        public required string Code { get; set; }
    }