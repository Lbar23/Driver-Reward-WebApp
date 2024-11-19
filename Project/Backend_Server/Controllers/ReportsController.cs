using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Backend_Server.Infrastructure;
using Serilog;

/// <summary>
/// I know you just added Reports Controller, but as usual, it did break (and will possibly break) future db migrations or implemtations
/// So, as a workaround, Use raw SQL queries with manual mapping instead... 
/// However, do not explicitly state them as HasNoKey in DB Context; same error will apply for future references if same key value name.
/// EF Core works weird w/ MySQL 
/// </summary>

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

        //Update for multiple sponsor specific to company...
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
                    if (viewType == "summary")
                    {
                        var result = await _context.Set<SalesSummary>()
                            .FromSqlRaw("CALL sp_GetSalesBySponsor({0}, {1}, {2}, {3})",
                                sponsorId ?? 0,
                                startDate ?? DateTime.MinValue,
                                endDate ?? DateTime.MaxValue,
                                viewType)
                            .AsNoTracking()
                            .ToListAsync();

                        return Ok(result);
                    }
                    else
                    {
                        var result = await _context.Set<SalesDetail>()
                            .FromSqlRaw("CALL sp_GetSalesBySponsor({0}, {1}, {2}, {3})",
                                sponsorId ?? 0,
                                startDate ?? DateTime.MinValue,
                                endDate ?? DateTime.MaxValue,
                                viewType)
                            .AsNoTracking()
                            .ToListAsync();

                        return Ok(result);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching sales by sponsor");
                return StatusCode(500, "Error fetching data");
            }
        }

        //Update for multiple sponsor specific to company...
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
                    if (viewType == "summary")
                    {
                        var result = await _context.Set<SalesSummary>()
                            .FromSqlRaw("CALL sp_GetSalesByDriver({0}, {1}, {2}, {3}, {4})",
                                sponsorId ?? 0,
                                driverId ?? 0,
                                startDate ?? DateTime.MinValue,
                                endDate ?? DateTime.MaxValue,
                                viewType)
                            .AsNoTracking()
                            .ToListAsync();

                        return Ok(result);
                    }
                    else
                    {
                        var result = await _context.Set<SalesDetail>()
                            .FromSqlRaw("CALL sp_GetSalesByDriver({0}, {1}, {2}, {3}, {4})",
                                sponsorId ?? 0,
                                driverId ?? 0,
                                startDate ?? DateTime.MinValue,
                                endDate ?? DateTime.MaxValue,
                                viewType)
                            .AsNoTracking()
                            .ToListAsync();

                        return Ok(result);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching sales by driver");
                return StatusCode(500, "Error fetching data");
            }
        }

        //Update for multiple sponsor specific to company...
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
                    var result = await _context.Set<InvoiceDetail>()
                        .FromSqlRaw("CALL sp_GetInvoiceReport({0}, {1}, {2})",
                            sponsorId ?? 0,
                            startDate ?? DateTime.MinValue,
                            endDate ?? DateTime.MaxValue)
                        .AsNoTracking()
                        .ToListAsync();

                    return Ok(result);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching invoice report");
                return StatusCode(500, "Error fetching data");
            }
        }

        //Update for multiple sponsor specific to company...
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
                    var result = await _context.Set<DriverPoints>()
                        .FromSqlRaw("CALL sp_GetDriverPointTracking({0}, {1}, {2}, {3})",
                            driverId ?? 0,
                            sponsorId ?? 0,
                            startDate ?? DateTime.MinValue,
                            endDate ?? DateTime.MaxValue)
                        .AsNoTracking()
                        .ToListAsync();

                    return Ok(result);
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching driver point tracking");
                return StatusCode(500, "Error fetching data");
            }
        }

        //Update for multiple sponsor specific to company...
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
