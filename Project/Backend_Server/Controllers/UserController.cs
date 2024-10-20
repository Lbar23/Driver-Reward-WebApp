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

    public class SponserAccessDto
    {
        public string AccessCode { get; set; } // <-- Access code based on Sponsor (unique specific ones for different Sponsors, but the same code for the same Sponsors)
    }
}