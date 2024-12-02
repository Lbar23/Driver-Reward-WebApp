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
using Microsoft.AspNetCore.Http.HttpResults;



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
    public class UserController(UserManager<Users> userManager, 
                                SignInManager<Users> signInManager, 
                                AppDBContext context, 
                                NotifyService notifyService, 
                                ClaimsService claimsService,
                                IMemoryCache cache) 
                                : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly SignInManager<Users> _signInManager = signInManager;
        private readonly AppDBContext _context = context;
        private readonly NotifyService _notifyService = notifyService;
        private readonly ClaimsService _claimsService = claimsService;

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
            var claims = await _userManager.GetClaimsAsync(user); // Fetch claims for the user

            object response;
            if (user.Role?.Name == "Sponsor") // Handle sponsor-specific details
            {
                var sponsorUser = await _context.SponsorUsers
                    .Include(su => su.Sponsor)
                    .SingleOrDefaultAsync(su => su.UserID == user.Id);

                response = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.CreatedAt,
                    user.LastLogin,
                    Roles = roles,
                    Claims = claims.Select(c => new { c.Type, c.Value }), // Add claims to response
                    SponsorDetails = sponsorUser != null ? new
                    {
                        sponsorUser.SponsorID,
                        sponsorUser.Sponsor.CompanyName,
                        sponsorUser.IsPrimary,
                        sponsorUser.JoinDate,
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
                    user.CreatedAt,
                    user.LastLogin,
                    Roles = roles,
                    Claims = claims.Select(c => new { c.Type, c.Value }) // Add claims to response
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
                
                await _claimsService.CreateAuditLog(
                    userId: user.Id,
                    category: AuditLogCategory.Password,
                    action: AuditLogAction.Update,
                    actionSuccess: result.Succeeded,
                    additionalDetails: result.Succeeded ? "Password changed successfully" : 
                        System.Text.Json.JsonSerializer.Serialize(result.Errors)
                );

                return BadRequest(result.Errors.Select(e => e.Description));
            }
            await _claimsService.CreateAuditLog(
                userId: user.Id,
                category: AuditLogCategory.Password,
                action: AuditLogAction.Update,
                actionSuccess: result.Succeeded,
                additionalDetails: result.Succeeded ? "Password changed successfully" : 
                    System.Text.Json.JsonSerializer.Serialize(result.Errors)
            );
            Log.Information("UserID: {UserID}, Category: User, Description: Password changed successfully for {User}",user.Id, user.UserName);
            return Ok(new { message = "Password changed successfully." });
        }

        [HttpPost("init-reset-password/{userId}")]
        public async Task<IActionResult> InitPasswordReset(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    Log.Warning("Password reset attempted for non-existent user ID: {UserId}", userId);
                    return NotFound("User not found.");
                }

                // Generate reset token
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                
                // Store token in cache with expiration
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                
                _cache.Set($"pwdreset_{userId}", resetToken, cacheOptions);

                // Send notification with reset token
                await _notifyService.NotifyAuthAsync(
                    user.Id,
                    resetToken,
                    user.UserName ?? "User"
                );

                Log.Information("Password reset initiated for UserID: {UserId}", userId);
                return Ok(new { message = "Password reset initiated. Check email for reset code." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initiate password reset for user {UserId}", userId);
                return StatusCode(500, "Failed to initiate password reset. Please try again.");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId.ToString());
                if (user == null)
                    return NotFound("User not found.");

                // Verify token from cache
                var cacheKey = $"pwdreset_{request.UserId}";
                if (!_cache.TryGetValue(cacheKey, out string? storedToken) || 
                    storedToken != request.Token)
                {
                    return BadRequest("Invalid or expired reset token.");
                }

                // Reset the password
                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
                if (!result.Succeeded)
                {
                    await _claimsService.CreateAuditLog(
                        userId: user.Id,
                        category: AuditLogCategory.Password,
                        action: AuditLogAction.Update,
                        actionSuccess: result.Succeeded,
                        additionalDetails: result.Succeeded ? "Password changed successfully" : 
                            System.Text.Json.JsonSerializer.Serialize(result.Errors)
                    );
                    return BadRequest(result.Errors.Select(e => e.Description));
                }

                // Cleanup
                _cache.Remove(cacheKey);

                Log.Information($"Password reset successful for user {request.UserId}");
                await _claimsService.CreateAuditLog(
                    userId: user.Id,
                    category: AuditLogCategory.Password,
                    action: AuditLogAction.Update,
                    actionSuccess: result.Succeeded,
                    additionalDetails: result.Succeeded ? "Password reset successfully" : 
                        System.Text.Json.JsonSerializer.Serialize(result.Errors)
                );
                return Ok(new { message = "Password reset successfully." });
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error resetting password for user {request.UserId}");
                return StatusCode(500, "Error resetting password. Please try again.");
            }
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