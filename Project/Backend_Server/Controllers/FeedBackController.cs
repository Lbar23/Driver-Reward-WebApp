using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Backend_Server.Models;
using Microsoft.Extensions.Caching.Memory;
using Backend_Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : CachedBaseController
    {
        private readonly AppDBContext _context;

        public FeedbackController(AppDBContext context, IMemoryCache cache) : base(cache)
        {
            _context = context;
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