using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend_Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DriverAppController(AppDBContext context, UserManager<Users> userManager) : ControllerBase
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _context = context;

        // //sumbit application async
        // [HttpPost("apply")]
        // public async Task<IActionResult> Apply()
        // {
        //     return Ok("Application submitted successfully!");
        // }
        // //status of application
        // [HttpGet("status/{id}")]
        // public async Task<IActionResult> GetApplicationStatus(int id)
        // {
        //     return Ok();
        // }

        // //update applicaiton
        // [HttpPut("update/{id}")]
        // public async Task<IActionResult> UpdateApplication(int id)
        // {
        //     return Ok("Application updated successfully!");
        // }

        // //pending
        // [HttpGet("pending")]
        // public async Task<IActionResult> GetPendingApplications()
        // {
        //     return Ok();
        // }
        
    }
}