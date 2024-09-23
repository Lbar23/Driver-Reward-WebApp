using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            var user = await _userService.RegisterUser(model.Username, model.Password, model.Email);
            return Ok(new { message = "User registered successfully", userId = user.UserId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class RegisterModel
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}