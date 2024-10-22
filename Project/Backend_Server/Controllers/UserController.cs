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
                        // Send via twilio
                        await _notifyService.SendSmsAsync(user.PhoneNumber, $"Your 2FA code is: {code}");
                        break;
                    case NotificationPref.Email:
                        // Send via sendgrid
                        var tData = new Dictionary<string, string>
                            {
                                { "auth_code", code}
                            };

                        await _notifyService.SendTemplateEmail(user.Email, "d-16815c0473d948acb2715a5001907e8c", tData);
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
                        Console.WriteLine($"Failed to send 2FA code to {user.UserName}");
                        return StatusCode(500, new { message = "Failed to send 2FA code. Please try again later." });
                    }

                    return Ok(new { message = "2FA required", userId = user.Id });
                }
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                Console.WriteLine($"User {user.UserName} logged in successfully.");
                return Ok(new { message = "Login successful", userId = user.Id, role = user.UserType });
            }
            Console.WriteLine($"Login failed for user {userDto.Username}");
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
                user.LastLogin
            });
        }

        //Gets all users
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.Select(u => new {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                UserType = u.UserType,
                LastLogin = u.LastLogin
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("drivers")]
        public async Task<IActionResult> GetAllDrivers()
        {
            Console.WriteLine("Testing");
            var drivers = await _userManager.Users
                .Where(u => u.UserType == "Driver")
                .Select(d => new {
                    Id = d.Id,
                    UserName = d.UserName,
                    Email = d.Email,
                    LastLogin = d.LastLogin,
                })
                .ToListAsync();

            return Ok(drivers);
        }



        [HttpPost("change-user-type")]
        public async Task<IActionResult> ChangeUserType([FromBody] ChangeUserTypeDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            //var currentRoles = await _userManager.GetRolesAsync(user);
            //await _userManager.RemoveFromRolesAsync(user, currentRoles);

            user.UserType = dto.NewUserType;
            //await _userManager.AddToRoleAsync(user, dto.NewUserType);
            await _userManager.UpdateAsync(user);

            return Ok(new { success = true, message = "User type updated successfully." });
        }

        [HttpDelete("remove-user/{id}")]
        public async Task<IActionResult> RemoveUser(int id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { success = true, message = "User removed successfully." });
            }

            return BadRequest(new { success = false, errors = result.Errors });
        }
        
        [HttpPost("impersonate")]
        public async Task<IActionResult> ImpersonateUser([FromBody] ImpersonateUserDto dto)
        {
            var adminUser = await _userManager.GetUserAsync(User);
            var targetUser = await _userManager.FindByIdAsync(dto.UserId.ToString());

            if (targetUser == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            // Store the admin's original identity
            var adminClaimsPrincipal = await _signInManager.CreateUserPrincipalAsync(adminUser);
            //await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Sign in as the target user
            await _signInManager.SignInAsync(targetUser, false);

            // Store the admin's identity in the session for later restoration
            HttpContext.Session.SetString("ImpersonatingAdminId", adminUser.Id.ToString());

            return Ok(new { success = true, message = "Impersonation successful", userId = targetUser.Id, role = targetUser.UserType });
        }

        [HttpPost("stop-impersonation")]
        public async Task<IActionResult> StopImpersonation()
        {
            var impersonatingAdminId = HttpContext.Session.GetString("ImpersonatingAdminId");
            if (string.IsNullOrEmpty(impersonatingAdminId))
            {
                return BadRequest(new { success = false, message = "No active impersonation session." });
            }

            var adminUser = await _userManager.FindByIdAsync(impersonatingAdminId);
            if (adminUser == null)
            {
                return NotFound(new { success = false, message = "Admin user not found." });
            }

            // Sign out the impersonated user
            //await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

            // Sign back in as the admin
            await _signInManager.SignInAsync(adminUser, false);

            // Clear the impersonation session data
            HttpContext.Session.Remove("ImpersonatingAdminId");

            return Ok(new { success = true, message = "Impersonation ended", userId = adminUser.Id, role = adminUser.UserType });
        }

        private static string DetermineUserRole(string registrationCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User not found.");

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserID == user.Id);

            if (driver == null)
                return NotFound("Driver not found.");

            // Poit transations
            var pointTransactions = await _context.PointTransactions
                .Where(t => t.DriverID == driver.DriverID)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionDto
                {
                    Date = t.TransactionDate,
                    Points = t.PointsChanged,
                    Type = "Point Change",
                    Reason = t.Reason,
                    SponsorName = t.Sponsor.CompanyName
                })
                .ToListAsync();

            // Purchases
            var purchases = await _context.Purchases
                .Where(p => p.DriverID == driver.DriverID)
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
                SponsorName = driver.Sponsor.CompanyName
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

    /********* DTO RECORD 'CLASSES' ***********/

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
}
public class TwoFactorDto
    {
        public required string UserId { get; set; }
        public required string Code { get; set; }
    
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
    public class ImpersonateUserDto
    {
        public int UserId { get; set; }
    }
    public class ChangeUserTypeDto
    {
        public int UserId { get; set; }
        public string NewUserType { get; set; }
    }
}