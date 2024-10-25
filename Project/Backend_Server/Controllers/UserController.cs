using System;
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

        /********* API CALL METHODS *********/
        
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
                        return StatusCode(500, new { succeeded = false, message = "Failed to send 2FA code. Please try again later." });
                    }

                    // 2FA is required
                    return Ok(new { succeeded = false, requiresTwoFactor = true, userId = user.Id });
                }

                // Login successful without 2FA
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new { succeeded = true, userId = user.Id, role = user.UserType });
            }

            // Failed login attempt
            return Unauthorized(new { succeeded = false, message = "Invalid username or password" });
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

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(new { message = "Password reset successfully." });
        }

        //uh, this is two stories combined, really
        //Since transactions and purchases are one, and really, getting the default value doesn't need a separate method
        //we ball with dis
        [HttpGet("activity")]
        public async Task<IActionResult> GetDriverActivity()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserID == user.Id);
            var sponsors = await _context.Sponsors.FirstOrDefaultAsync(s => s.UserID == user.Id);

            if (driver == null)
                return NotFound("Driver not found.");

            // Poit transations
            var pointTransactions = await _context.PointTransactions
                .Where(t => t.UserID == driver.UserID)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionDto
                {
                    Date = t.TransactionDate,
                    Points = t.PointsChanged,
                    Type = "Point Change",
                    Reason = t.Reason,
                    SponsorName = sponsors!.CompanyName
                })
                .ToListAsync();

            // Purchases
            var purchases = await _context.Purchases
                .Where(p => p.UserID == driver.UserID)
                .OrderByDescending(p => p.PurchaseDate)
                .Select(p => new TransactionDto
                {
                    Date = p.PurchaseDate,
                    Points = -p.PointsSpent, // negative points be like (spent)
                    Type = "Purchase",
                    Reason = $"Purchased {p.Product.Name}",
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            // Sorting transactions
            var allTransactions = pointTransactions.Concat(purchases)
                .OrderByDescending(t => t.Date)
                .ToList();

            // Le Sponsor point value
            var pointValue = new PointValueDto
            {
                TotalPoints = driver.TotalPoints,
                PointValue = 0.01M, // Standalone default vaule; change based on project requirements/bare minimum
                SponsorName = sponsors!.CompanyName
            };

            return Ok(new
            {
                PointValue = pointValue,
                Transactions = allTransactions
            });
        }
        
        /********* ASYNC FUNCTIONS CODE ****************/

        //Permission task to grab the entire list of specific permissions for the specified user(s)
        private async Task<List<string>> GetUserPermissions(Users user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _context.Permissions 
                .Where(p => roles.Contains(p.Role))
                .Select(p => p.Permission.ToString())
                .Distinct()
                .ToListAsync();
            return permissions;
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

    public record TransactionDto
    {
        public DateTime Date { get; init; }
        public int Points { get; init; }
        public required string Type { get; init; }
        public required string Reason { get; init; }
        public string? SponsorName { get; init; }
        public string? Status { get; init; }
    }

    public record PointValueDto
    {
        public int TotalPoints { get; init; }
        public decimal PointValue { get; init; }
        public required string SponsorName { get; init; }
        public decimal TotalValue => TotalPoints * PointValue;
    }
}