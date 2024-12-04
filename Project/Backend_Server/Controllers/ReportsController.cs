using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Backend_Server.Infrastructure;
using Serilog;
using MySqlConnector;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Backend_Server.Services;


namespace Backend_Server.Controllers
{
    /// <summary>
    /// ReportsController:
    /// 
    /// This controller handles all report functionalities, including report procedure calls, 
    ///  audit logs, and file exports.
    ///
    /// Endpoints:
    
    /// [PUT]   /api/reports/sales-sponsor                  - fetches sponsor sales report data from db procedure
    /// [GET]   /api/reports/sales-driver                   - fetches driver sales report data from db procedure
    /// [GET]   /api/reports/invoice                        - fetches invoice report data from db procedure
    /// [GET]   /api/reports/driver-points                  - fetches driver points activity data from db procedure
    /// [GET]   /api/reports/audit-logs                     - fetches audit log report data from db procedure [NOT YET - will update]
    /// [POST]   /api/reports/export-pdf                    - general api for creating a pdf 
    /// [POST]   /api/reports/export-csv                    - general api for creating a csv
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDBContext _context;
        private readonly ReportService _reportService;

        public ReportsController(UserManager<Users> userManager, 
                                 AppDBContext context,
                                 ReportService reportService)
        {
            _userManager = userManager;
            _context = context;
            _reportService = reportService;
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    Log.Warning($"Retry {attempt}/{maxRetries} for operation failed: {ex.Message}");
                }
            }

            throw new InvalidOperationException("Max retries exceeded for operation.");
        }

        /// <summary>
        /// Retrieves sales data by sponsor, either in summary or detailed view.
        /// </summary>
        [HttpGet("sales-sponsor")]
        public async Task<IActionResult> sp_GetSalesBySponsor(
            [FromQuery] int? sponsorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string viewType)
        {
            try
            {
                if (viewType == "summary")
                {
                    // Fetch summary data
                    var summaryResult = await _reportService.GetSponsorSalesSummary(sponsorId, startDate, endDate, "summary");
                    return Ok(summaryResult);
                }
                else if (viewType == "detail")
                {
                    var detailedResult = await _reportService.GetSponsorSalesDetail(sponsorId, startDate, endDate, "detail");
                    return Ok(detailedResult);
                }
                else
                {
                    return BadRequest("Invalid viewType parameter. Must be 'summary' or 'detailed'.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching sales by sponsor");
                return StatusCode(500, "Error fetching data");
            }
        }

        /// <summary>
        /// Retrieves sales data by driver, either in summary or detailed view.
        /// </summary>
        [HttpGet("sales-driver")]
        public async Task<IActionResult> sp_GetSalesByDriver(
            [FromQuery] int? sponsorId,
            [FromQuery] int? driverId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string viewType)
        {
            try
            {
                return await ExecuteWithRetryAsync<IActionResult>(async () =>
                {
                    if (viewType == "summary")
                    {
                        // Fetch summary data
                        var summaryResult = await _reportService.GetDriverSalesSummary(sponsorId, driverId, startDate, endDate, "summary");
                        return Ok(summaryResult);
                    }
                    else if (viewType == "detail")
                    {
                        var detailedResult = await _reportService.GetDriverSalesDetail(sponsorId, driverId, startDate, endDate, "detail");
                        return Ok(detailedResult);
                    }
                    else
                    {
                        return BadRequest("Invalid viewType parameter. Must be 'summary' or 'detailed'.");
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching sales by driver");
                return StatusCode(500, "Error fetching data");
            }
        }


        [HttpGet("invoice")]
        public async Task<IActionResult> GetInvoiceReport(
            [FromQuery] int? sponsorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                return await ExecuteWithRetryAsync(async () =>
                {
                    var result = await _context.Set<InvoiceDetail>().FromSqlRaw(
                        "CALL sp_GetInvoiceReport(@sponsorId, @startDate, @endDate)",
                        new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                        new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                        new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value)
                    ).ToListAsync();

                    return Ok(result);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching invoice report");
                return StatusCode(500, "Error fetching data");
            }
        }

        [HttpGet("driver-points")]
        public async Task<IActionResult> sp_GetDriverPointTracking(
            [FromQuery] int? driverId,
            [FromQuery] int? sponsorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                return await ExecuteWithRetryAsync(async () =>
                {
                    var result = await _context.Set<DriverPoints>().FromSqlRaw(
                        "CALL sp_GetDriverPointTracking(@driverId, @sponsorId, @startDate, @endDate)",
                        new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                        new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                        new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                        new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value)
                    ).ToListAsync();

                    return Ok(result);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching driver points tracking");
                return StatusCode(500, "Error fetching data");
            }
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int? userId = null,
            [FromQuery] string? category = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized("User not found");
                }

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                
                // Start with the base query
                var query = _context.AuditLogs
                    .Select(l => new { // Project to anonymous type to avoid circular references
                        l.LogID,
                        l.UserID,
                        UserName = l.User.UserName,
                        l.Category,
                        l.Action,
                        l.ActionSuccess,
                        l.Timestamp,
                        l.AdditionalDetails
                    })
                    .AsQueryable();

                // Apply sponsor filtering if applicable
                if (userRoles.Contains("Sponsor"))
                {
                    var sponsorUser = await _context.SponsorUsers
                        .FirstOrDefaultAsync(su => su.UserID == currentUser.Id);

                    if (sponsorUser == null)
                        return BadRequest("Sponsor user not found");

                    // Get all drivers under this sponsor
                    var driverIds = await _context.SponsorDrivers
                        .Where(sd => sd.SponsorID == sponsorUser.SponsorID)
                        .Select(sd => sd.UserID)
                        .ToListAsync();

                    query = query.Where(l => driverIds.Contains(l.UserID));
                }

                // Apply filters
                if (userId.HasValue)
                    query = query.Where(l => l.UserID == userId);

                if (!string.IsNullOrEmpty(category) && Enum.TryParse(category, out AuditLogCategory categoryEnum))
                    query = query.Where(l => l.Category == categoryEnum);

                if (startDate.HasValue)
                    query = query.Where(l => l.Timestamp >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(l => l.Timestamp <= endDate.Value);

                // Get total count and paginated results
                var totalCount = await query.CountAsync();
                var logs = await query
                    .OrderByDescending(l => l.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new { totalCount, page, pageSize, logs });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching audit logs");
                return StatusCode(500, "Error retrieving audit logs");
            }
        }

        [HttpPost("export-pdf")]
        public IActionResult ExportPdf([FromBody] ExportRequest request)
        {
            try{
                if (request == null)
                    return BadRequest("Request cannot be null");

                var bytes = _reportService.ExportPdf(request);  
                return File(bytes, "application/pdf", $"{request.ReportType}_report.pdf");
            }
            catch (Exception ex){
                Log.Error(ex, "Error generating PDF report");
                return StatusCode(500, "Failed to generate PDF report.");
            }
        }

        [HttpPost("export-csv")]
        public IActionResult ExportCsv([FromBody] ExportRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest("Request cannot be null");

                var bytes = _reportService.ExportCsv(request);
                return File(bytes, "text/csv", $"{request.ReportType}_report.csv");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating CSV report");
                return StatusCode(500, "Failed to generate CSV report.");
            }
        }

    }
}
