using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using Team16_WebApp_4910.Server;
using Team16_WebApp_4910.Server.Models;

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
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}