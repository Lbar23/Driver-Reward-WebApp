using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Backend_Server;
using Backend_Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Server.Controllers{


    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDBContext _context;

        public AdminController(AppDBContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAdmin([FromBody] AdminCreateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new Users
            {
                UserName = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                UserType = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Admin user created successfully" });
        }

        [HttpGet("about")]
        public async Task<IActionResult> GetAbout()
        {
            var aboutInfo = await _context.About.LastOrDefaultAsync();
            if (aboutInfo == null)
            {
                return NotFound(new { message = "No about information found" });
            }

            return Ok(new
            {
                aboutInfo.Team,
                aboutInfo.Version,
                aboutInfo.Release,
                aboutInfo.Product,
                aboutInfo.Description,
            });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }

    public class AdminCreateModel
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    };

}