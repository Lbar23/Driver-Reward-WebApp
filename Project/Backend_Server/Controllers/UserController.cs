using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Backend_Server.Services;
using Serilog;
using Backend_Server.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<Users> userManager, SignInManager<Users> signInManager, AppDBContext context, NotifyService notifyService, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly AppDBContext _context = context;
        private readonly NotifyService _notifyService = notifyService;

        /********* API CALL METHODS *********/
        
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            // do later lmao if (isAdminDomain) user's email == web app default admin domain; UserType = UserType.Admin.ToString()
            var user = new Users
            {
                UserName = userDto.Username,
                Email = userDto.Email,
                UserType = UserType.Guest.ToString(),
                CreatedAt = DateTime.UtcNow,
                NotifyPref = NotificationPref.Email,
                EmailConfirmed = true, //for now
                PhoneNumberConfirmed = false,
                LockoutEnabled = false,
                TwoFactorEnabled = userDto.Enable2FA, // Set 2FA based on user's choice during registration
                AccessFailedCount = 0
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, user.UserType);
                Log.Information("User registered correctly! {Username}", user.UserName);

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
                Log.Error("User registration failed");
                return BadRequest(result.Errors);
            }

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
                            await _notifyService.SendTemplateEmail(user.Email, "d-16815c0473d948acb2715a5001907e8c", tData);
                            return true;
                        }
                        break;
                }

                return false;
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
                        Log.Error("Failed to send 2FA code to {User}", user.UserName);
                        return StatusCode(500, new { succeeded = false, message = "Failed to send 2FA code. Please try again later." });
                    }

                    // 2FA is required
                    return Ok(new { succeeded = false, requiresTwoFactor = true, userId = user.Id });
                }

                // Login successful without 2FA
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                Log.Information("User {User} logged in successfully", user.UserName);
                return Ok(new { message = "Login successful", userId = user.Id, role = user.UserType, succeeded = true });
            }

            Log.Error("Login Failed for {UserName}", user?.UserName);
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
                Log.Information("{Username} of type {User} has successfully logged out", user?.UserName, user?.UserType.ToString());
                await _context.SaveChangesAsync();
            }
            else
            {
                Log.Error("{Username} of type {User} has failed to log out correctly", user?.UserName, user?.UserType.ToString());
                return StatusCode(500, "Failed to log out user");
            }
            
            return Ok(new { message = "Logged out successfully", succeeded = true});
        }

        // [Authorize] // has to be protected
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
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.UserType,
                user.CreatedAt,
                user.LastLogin,
                Roles = roles,
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
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.UserType,
                user.CreatedAt,
                user.LastLogin,
                Roles = roles,
            });
            
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Error("There is no user with username, please try again");
                return NotFound("User not found, Please try again.");
            }

            var result = await _userManager.ChangePasswordAsync(user, 
                request.CurrentPassword, 
                request.NewPassword);

            if (!result.Succeeded)
            {
                Log.Error("Password changed failed for {User}", user.UserName);
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            //Ugh, in Sprint 9 of project, do manual logging for better logging levels; as of now, basic http requests auto logging
            //for EVERY return, wooooooooooooooooo
            Log.Information("Password changed successfully for {User}", user.UserName);
            return Ok(new { message = "Password changed successfully." });
        }

        //Doesn't have an frontend api page yet; just here when it happens
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
            Log.Information("Token generated.");

            Log.Information("Waiting on Notification System...");

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            Log.Information("Password for {User} has been reset successfully", user.UserName);
            return Ok(new { message = "Password reset successfully." });
        }
        
        /********* ASYNC FUNCTIONS CODE ****************/

        // commented out b/c permissions table removed
        //Permission task to grab the entire list of specific permissions for the specified user(s)
        // private async Task<List<string>> GetUserPermissions(Users user)
        // {
        //     var roles = await _userManager.GetRolesAsync(user);
        //     var permissions = await _context.Permissions 
        //         .Where(p => roles.Contains(p.Role))
        //         .Select(p => p.Permission.ToString())
        //         .Distinct()
        //         .ToListAsync();
        //     return permissions;
        // }

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

    /********* DTO RECORD 'CLASSES' ***********/

    public record UserRegisterDto //For better scalability, turn every DTO going forward into records instead (since they are inherently immutable data carriers)
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool Enable2FA { get; set; } 
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
    public class TwoFactorDto
    {
        public required string UserId { get; set; }
        public required string Code { get; set; }
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