using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend_Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Backend_Server.Infrastructure;

namespace Backend_Server.Controllers
{
    /// <summary>
    /// DriverAppController:
    /// 
    /// This controller manages driver applications, allowing users to apply, view their application status, 
    /// update applications, and retrieve pending applications for review.
    ///
    /// Endpoints:
    /// 
    /// [POST] /api/driverapp/apply         - Submits a new driver application
    /// [GET]  /api/driverapp/status/{id}   - Retrieves the status of a specific application
    /// [PUT]  /api/driverapp/update/{id}   - Updates the status and details of a specific application
    /// [GET]  /api/driverapp/pending       - Retrieves all pending applications
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DriverAppController(AppDBContext context, UserManager<Users> userManager, IMemoryCache cache) : CachedBaseController(cache)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;

        //sumbit application async
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] DriverApplications application)
        {
            var user = await _userManager.GetUserAsync(User); // Get the current logged-in user

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // automatically assign the user's ID to the application 
            application.UserID = user.Id;
            application.Status = AppStatus.Submitted;
            application.ApplyDate = DateOnly.FromDateTime(DateTime.UtcNow);

            _context.DriverApplications.Add(application);
            await _context.SaveChangesAsync();
            return Ok("Application submitted successfully!");
        }
        
        //status of application
        [HttpGet("status/{id}")]
        public async Task<IActionResult> GetApplicationStatus(int id)
        {
            var application = await _context.DriverApplications.FindAsync(id);
            if (application == null)
            {
                return NotFound("Application not found.");
            }
            return Ok(application.Status);
        }

        //update applicaiton
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] DriverApplications updatedApplication)
        {
            var application = await _context.DriverApplications.FindAsync(id);
            if (application == null)
            {
                return NotFound("Application not found.");
            }

            application.Status = updatedApplication.Status;
            application.ProcessedDate = updatedApplication.ProcessedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            application.Reason = updatedApplication.Reason;
            _context.DriverApplications.Update(application);
            await _context.SaveChangesAsync();
            return Ok("Application updated successfully!");
        }

        //pending
        [HttpGet("pending")]
         public async Task<IActionResult> GetPendingApplications()
        {
            var pendingApplications = await _context.DriverApplications
                .Where(app => app.Status == AppStatus.Submitted)
                .ToListAsync();
            return Ok(pendingApplications);
        }
        
    }
}