using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Team16_WebApp_4910.Server.Models;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Net;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Team16_WebApp_4910.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<Users> userManager, SignInManager<Users> signInManager, Iconfiguration configuration, IEmailService, emailService) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly IEmailService _emailService = emailService;

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
                //await SendConfirmationEmail(user.Email);
                return Ok(new { success = true, message = "User registered successfully. Please check your email to confirm your account.", role = user.UserType });
            }

            return BadRequest(new { success = false, errors = result.Errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var user = await _userManager.FindByNameAsync(userDto.Username)
                        ?? await _userManager.FindByEmailAsync(userDto.Username);
            
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Invalid username or password" });
            }
            /*
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized(new { 
                    success = false, 
                    message = "Email not confirmed", 
                    requiresEmailConfirmation = true 
                });
            }*/

            var result = await _signInManager.PasswordSignInAsync(user, userDto.Password, userDto.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                
                return Ok(new { success = true, message = "Login successful", userId = user.Id, role = user.UserType });
            }
            
            if (result.RequiresTwoFactor)
            {
                return Ok(new { success = true, requiresTwoFactor = true, userId = user.Id });
            }

            if (result.IsLockedOut)
            {
                return BadRequest(new { success = false, message = "User account locked out" });
            }

            return Unauthorized(new { success = false, message = "Invalid username or password" });
        }

        [HttpPost("send-confirmation-email")]
        public async Task<IActionResult> SendConfirmationEmail([FromBody] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok("If a user with this email exists, a confirmation email has been sent.");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest("This email is already confirmed.");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                new { userId = user.Id, token = token }, Request.Scheme);

            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Confirm your email",
                    $"Please confirm your account by clicking this link: {confirmationLink}"
                );

                return Ok("Confirmation email sent. Please check your email.");
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, "Failed to send confirmation email. Please try again.");
            }
        }

        [HttpPost("send-2fa-code")]
        public async Task<IActionResult> SendTwoFactorCode([FromBody] SendTwoFactorCodeDto dto)
        {
            Console.WriteLine(dto.UserId);
            try
            {
                Console.WriteLine(dto.UserId);
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
                
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found." });
                }

                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Your Two-Factor Authentication Code",
                    $"Your two-factor authentication code is: {token}"
                );

                return Ok(new { success = true, message = "2FA code sent to your email." });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An error occurred on the server." });
            }
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor(TwoFactorDto twoFactorDto)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == twoFactorDto.UserId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", twoFactorDto.Token);

            if (isValid)
            {
                await _signInManager.SignInAsync(user, twoFactorDto.RememberMe);
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                return Ok(new { success = true, message = "Login successful", userId = user.Id, role = user.UserType });
            }

            return Unauthorized(new { success = false, message = "Invalid verification code." });
        }

        [HttpPost("google-sign-in")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { 446234699257-5k91m28pvfpk6mov3gr9pi190p261d8r.apps.googleusercontent.com }
                });

                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    user = new Users
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        UserType = "Driver", // Default role, adjust as needed
                        CreatedAt = DateTime.UtcNow
                    };
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { success = false, errors = result.Errors });
                    }
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok(new { success = true, message = "Google sign-in successful", userId = user.Id, role = user.UserType });
            }
            catch (InvalidJwtException)
            {
                return BadRequest(new { success = false, message = "Invalid Google token", details = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { success = true, message = "Logged out successfully" });
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
        public string CompanyName { get; set; }
    }

    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class SendTwoFactorCodeDto
    {
        public int UserId { get; set; }
    }

    public class TwoFactorDto
    {
        public int UserId { get; set; }
        public string Token { get; set; }
        public bool RememberMe { get; set; }
    }

    public class GoogleSignInRequest
    {
        public string Token { get; set; }
    }
}    
