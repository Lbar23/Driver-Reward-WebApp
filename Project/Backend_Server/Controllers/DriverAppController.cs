using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend_Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverAppController(AppDBContext context, UserManager<Users> userManager) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;

        //sumbit application async
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] DriverApplications application)
        {
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