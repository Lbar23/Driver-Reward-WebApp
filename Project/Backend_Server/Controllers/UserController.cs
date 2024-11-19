using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Backend_Server.Services;
using Serilog;
using Backend_Server.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;



namespace Backend_Server.Controllers
{
    /// <summary>
    /// UserController:
    /// 
    /// This controller manages user-related actions like feedback submission or profile updates (when added)
    /// and user account management such as password changes and role retrieval.
    ///
    /// Endpoints:
    /// 
    /// [GET]  /api/user/currentuser         - Retrieves the currently logged-in user's details
    /// [GET]  /api/user/getuser             - Retrieves user details by user ID
    /// [POST] /api/user/change-password     - Changes the current user's password
    /// [POST] /api/user/reset-password      - Resets a user's password (requires admin access)
    /// [POST] /api/user/submit-feedback     - Submits feedback from a user
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserManager<Users> userManager, SignInManager<Users> signInManager, AppDBContext context, NotifyService notifyService, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly AppDBContext _context = context;
        private readonly NotifyService _notifyService = notifyService;

        /********* API CALLS *********/

        [HttpGet("currentuser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User is not logged in");
            }

            var roles = await _userManager.GetRolesAsync(user);

            object response;
            if (user.UserType == "Sponsor") //This check has to be here since we're dealing with multiple sponsors under the same company...
            {
                var sponsorUser = await _context.SponsorUsers
                    .Include(su => su.Sponsor)
                    .FirstOrDefaultAsync(su => su.UserID == user.Id);

                response = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.UserType,
                    user.CreatedAt,
                    user.LastLogin,
                    Roles = roles,
                    SponsorDetails = sponsorUser != null ? new
                    {
                        sponsorUser.SponsorID,
                        sponsorUser.Sponsor.CompanyName,
                        sponsorUser.IsPrimarySponsor,
                        sponsorUser.JoinDate,
                        sponsorUser.SponsorRole
                    } : null
                };
            }
            else
            {
                response = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.UserType,
                    user.CreatedAt,
                    user.LastLogin,
                    Roles = roles
                };
            }

            return Ok(response);
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
            object response;
            if (user.UserType == "Sponsor") //Same here
            {
                var sponsorUser = await _context.SponsorUsers
                    .Include(su => su.Sponsor)
                    .FirstOrDefaultAsync(su => su.UserID == user.Id);

                response = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.UserType,
                    user.CreatedAt,
                    user.LastLogin,
                    Roles = roles,
                    SponsorDetails = sponsorUser != null ? new
                    {
                        sponsorUser.SponsorID,
                        sponsorUser.Sponsor.CompanyName,
                        sponsorUser.IsPrimarySponsor,
                        sponsorUser.JoinDate,
                        sponsorUser.SponsorRole
                    } : null
                };
            }
            else
            {
                response = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.UserType,
                    user.CreatedAt,
                    user.LastLogin,
                    Roles = roles
                };
            }

            return Ok(response);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Log.Error("UserID: N/A, Category: User, Description: There is no user with username, please try again");
                return NotFound("User not found, Please try again.");
            }

            var result = await _userManager.ChangePasswordAsync(user, 
                request.CurrentPassword, 
                request.NewPassword);

            if (!result.Succeeded)
            {
                Log.Error("UserID: {UserID}, Category: User, Description: Password changed failed for {User}",user.Id, user.UserName);
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            //Ugh, in Sprint 9 of project, do manual logging for better logging levels; as of now, basic http requests auto logging
            //for EVERY return, wooooooooooooooooo
            Log.Information("UserID: {UserID}, Category: User, Description: Password changed successfully for {User}",user.Id, user.UserName);
            return Ok(new { message = "Password changed successfully." });
        }

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
            Log.Information("Token generated.");

            Log.Information("Waiting on Notification System...");

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }
            Log.Information("UserID: {UserID}, Category: User, Description: Password for {User} has been reset successfully", user.Id, user.UserName);
            return Ok(new { message = "Password reset successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback(FeedbackForms feedback)
        {
            try
            {
                feedback.SubmissionDate = DateTime.UtcNow;
                await _context.FeedbackForms.AddAsync(feedback);
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Feedback submitted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting feedback: {ex.Message}");
                return StatusCode(500, "An error occurred while submitting your feedback. Please try again later.");
            }
        }
    }
}