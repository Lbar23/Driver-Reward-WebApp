using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team16_WebApp_4910.Server.Models;

namespace Team16_WebApp_4910.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDBContext _context;

        public UserController(AppDBContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto userDto)
        {
            var user = new Users
            {
                Username = userDto.Username,
                Email = userDto.Email,
                PasswordHash = userDto.Password, // Note: This is not secure. We'll implement hashing later w/ Identity to follow.
                UserType = DetermineUserRole(userDto.RegistrationCode),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", role = user.UserType });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

            if (user == null || user.PasswordHash != userDto.Password) // Note: This is not secure. We'll implement proper verification later.
            {
                return Unauthorized("Invalid username or password");
            }

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Login successful", userId = user.UserID, role = user.UserType });
        }

        [HttpGet("test-db-connection")]
        public async Task<IActionResult> TestDbConnection()
        {
            try
            {
                var userCount = await _context.Users.CountAsync();
                return Ok($"Connection successful. User count: {userCount}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.UserID,
                user.Username,
                user.Email,
                user.UserType,
                user.CreatedAt,
                user.LastLogin
            });
        }

        private string DetermineUserRole(string registrationCode)
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
        public string CompanyName { get; set; }
    }

    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}