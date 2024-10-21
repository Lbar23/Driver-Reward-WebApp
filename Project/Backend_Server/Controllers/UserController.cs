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
    public class UserController(UserManager<Users> userManager, SignInManager<Users> signInManager) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;

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

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(userDto.Username);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.UserType,
                user.CreatedAt,
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
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RegistrationCode { get; set; }
        //public string CompanyName { get; set; }
    }

    public class UserLoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
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