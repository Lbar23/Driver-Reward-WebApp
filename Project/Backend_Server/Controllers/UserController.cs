using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<Users> userManager, SignInManager<Users> signInManager, AppDBContext context) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly AppDBContext _context = context;

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
                    //need to add email option
                    var code = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
                    // setup sns later here
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

        [HttpGet("currentuser")]
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

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found, Please try again.");
            }

            var result = await _userManager.ChangePasswordAsync(user, 
                request.CurrentPassword, 
                request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            //Ugh, in Sprint 9 of project, do manual logging for better logging levels; as of now, basic http requests auto logging
            //for EVERY return, wooooooooooooooooo
            return Ok(new { message = "Password changed successfully." });
        }

        //Doesn't have an frontend api call yet; just here when it happens
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetUserPassword(string userId, [FromBody] ResetPasswordDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found, Please try again.");
            }

            // Generate reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(new { message = "Password reset successfully." });
        }

        //Permission task to grab the entire list of specific permissions for the specified user(s)
        private async Task<List<string>> GetUserPermissions(Users user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _context.Permissions // <-- Will throw error until I update database & AppDBContext (remove once added/fixed with no errors)
                .Where(p => roles.Contains(p.Role))
                .Select(p => p.PermissionName)
                .Distinct()
                .ToListAsync();
            return permissions;
        }

        //Replaced with unique admin domain email instead of registration code
        private static string DetermineUserRole(string email)
        {
            //Don't have an official one yet. Unless you want me to set it up while I touch S3 as well; 
            //I did know you brought it AWS Route 53, though.
            string adminDomain = "admin.domain.com"; // <-- username@admin.domainname.com

            if (adminDomain.Any(domain => email.EndsWith("@" + domain)))
            {
                return "Admin";
            }
            
            return "Driver"; // The overall Default role of the 3 users
        }
    }

    public record UserRegisterDto //For better scalability, turn every DTO going forward into records instead (since they are inherently immutable data carriers)
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string RegistrationCode { get; set; }
        //public string CompanyName { get; set; }
    }

    public record UserLoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public record SponserAccessDto
    {
        public required string AccessCode { get; set; } // <-- Access code based on Sponsor (unique specific ones for different Sponsors, but the same code for the same Sponsors)
    }

    public record ResetPasswordDto 
    {
        public required string NewPassword { get; set; }
    }

    public record ChangePasswordDto
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}