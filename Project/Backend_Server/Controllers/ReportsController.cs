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

namespace Backend_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly AppDBContext _context;

        public ReportsController(UserManager<Users> userManager, AppDBContext context)
        {
            _userManager = userManager;
            _context = context;
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

        [HttpGet("sales-sponsor")]
        public async Task<IActionResult> sp_GetSalesBySponsor(
            [FromQuery] int? sponsorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string viewType = "summary")
        {
            try
            {
                return await ExecuteWithRetryAsync(async () =>
                {
                    var result = await _context.Set<SalesSummary>().FromSqlRaw(
                        "CALL sp_GetSalesBySponsor(@sponsorId, @startDate, @endDate, @viewType)",
                        new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                        new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                        new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                        new MySqlParameter("@viewType", viewType ?? "summary")
                    ).ToListAsync();

                    return Ok(result);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching sales by sponsor");
                return StatusCode(500, "Error fetching data");
            }
        }

        [HttpGet("sales-driver")]
        public async Task<IActionResult> sp_GetSalesByDriver(
            [FromQuery] int? sponsorId,
            [FromQuery] int? driverId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string viewType = "summary")
        {
            try
            {
                return await ExecuteWithRetryAsync(async () =>
                {
                    var result = await _context.Set<SalesDetail>().FromSqlRaw(
                        "CALL sp_GetSalesByDriver(@sponsorId, @driverId, @startDate, @endDate, @viewType)",
                        new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                        new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                        new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                        new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                        new MySqlParameter("@viewType", viewType ?? "summary")
                    ).ToListAsync();

                    return Ok(result);
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
            [FromQuery] int? sponsorId,
            [FromQuery] int? driverId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                return await ExecuteWithRetryAsync(async () =>
                {
                    var result = await _context.Set<DriverPoints>().FromSqlRaw(
                        "CALL sp_GetDriverPointTracking(@sponsorId, @driverId, @startDate, @endDate)",
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
                return await ExecuteWithRetryAsync(async () =>
                {
                    var query = _context.AuditLogs.AsQueryable();

                    if (userId.HasValue)
                        query = query.Where(l => l.UserID == userId);

                    if (!string.IsNullOrEmpty(category) && Enum.TryParse(category, out AuditLogCategory categoryEnum))
                        query = query.Where(l => l.Category == categoryEnum);

                    if (startDate.HasValue)
                        query = query.Where(l => l.Timestamp >= startDate.Value);

                    if (endDate.HasValue)
                        query = query.Where(l => l.Timestamp <= endDate.Value);

                    var totalCount = await query.CountAsync();
                    var logs = await query
                        .OrderByDescending(l => l.Timestamp)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    return Ok(new { totalCount, page, pageSize, logs });
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching audit logs");
                return StatusCode(500, "Error retrieving audit logs");
            }
        }
    }
}
