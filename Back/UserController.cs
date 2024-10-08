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


namespace Team16_WebApp_4910.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly AppDBContext _context;
        private static readonly Dictionary<string, string> _otpStore = new Dictionary<string, string>();

        public UserController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            AppDBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate and send verification code
            return Ok(new { message = "User registered successfully. Please check your email for the verification code.", userId = user.UserID });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userDto.Username);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            // Generate and send OTP
            string otp = GenerateOTP();
            _otpStore[user.Email] = otp;
            await SendOTPEmail(user.Email, otp);

            return Ok(new { message = "OTP sent to email", requiresOTP = true });

        }
        
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP(OTPVerificationDto otpDto)
        {
            if (_otpStore.TryGetValue(otpDto.Email, out string storedOtp) && storedOtp == otpDto.OTP)
            {
                _otpStore.Remove(otpDto.Email);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == otpDto.Email);
                return Ok(new { message = "Login successful", userId = user.UserID, role = user.UserType });
            }

            return Unauthorized("Invalid OTP");
        }

        [HttpPost("enable-2fa")]
        public async Task<IActionResult> EnableTwoFactor()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                // In a real application, you would send this token via email
                // For demonstration purposes, we're returning it directly
                return Ok(new { twoFactorToken = token });
            }

            return BadRequest("Two-factor authentication is already enabled.");
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor(TwoFactorDto twoFactorDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", twoFactorDto.Token);
            if (result)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                return Ok(new { message = "Two-factor authentication enabled successfully." });
            }

            return BadRequest("Invalid verification code.");
        }

        [HttpPost("login-2fa")]
        public async Task<IActionResult> LoginTwoFactor(TwoFactorDto twoFactorDto)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", twoFactorDto.Token, false, false);

            if (result.Succeeded)
            {
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Login successful", userId = user.Id, role = user.UserType });
            }

            return Unauthorized("Invalid verification code.");
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

        [HttpPost("google-sign-in")]
        public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { "446234699257-5k91m28pvfpk6mov3gr9pi190p261d8r.apps.googleusercontent.com" } // Replace with your Google Client ID
                });

                // Check if the user exists in your database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    // Create a new user if they don't exist
                    user = new Users
                    {
                        Email = payload.Email,
                        Username = payload.Email, // You might want to generate a username or ask the user to create one
                        // Set other properties as needed
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Log the user in
                return Ok(new { success = true, message = "Google sign-in successful", userId = user.UserID, role = user.UserType });
            }
            catch (InvalidJwtException)
            {
                return BadRequest(new { success = false, message = "Invalid Google token" });
            }
        }


        private string GenerateOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private async Task SendOTPEmail(string email, string otp)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("your-email@gmail.com"),
                Subject = "Your Login OTP",
                Body = $"Your OTP is: {otp}",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
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

    public class TwoFactorDto
    {
        public string Token { get; set; }
    }
    public class OTPVerificationDto
    {
        public string Email { get; set; }
        public string OTP { get; set; }
    }
    public class GoogleSignInRequest
    {
        public string Token { get; set; }
    }
/*
    public class ApplicationUser : IdentityUser
    {
        public string UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
    */
}