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

        /// <summary>
        /// Retrieves sales data by sponsor, either in summary or detailed view.
        /// </summary>
        [HttpGet("sales-sponsor")]
        public async Task<IActionResult> sp_GetSalesBySponsor(
            [FromQuery] int? sponsorId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string viewType = "summary")
        {
            try
            {
                return await ExecuteWithRetryAsync<IActionResult>(async () =>
                {
                    if (viewType == "summary")
                    {
                        // Fetch summary data
                        var summaryResult = await _context.Set<SpSalesSummary>().FromSqlRaw(
                            "CALL sp_GetSalesBySponsor(@sponsorId, @startDate, @endDate, @viewType)",
                            new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                            new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                            new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                            new MySqlParameter("@viewType", viewType)
                        ).ToListAsync();

                        return Ok(summaryResult);
                    }
                    else if (viewType == "detail")
                    {
                        // Fetch detailed data
                        var detailedResult = await _context.Set<SalesDetail>().FromSqlRaw(
                            "CALL sp_GetSalesBySponsor(@sponsorId, @startDate, @endDate, @viewType)",
                            new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                            new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                            new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                            new MySqlParameter("@viewType", viewType)
                        ).ToListAsync();

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
            [FromQuery] string viewType = "summary")
        {
            try
            {
                return await ExecuteWithRetryAsync<IActionResult>(async () =>
                {
                    if (viewType == "summary")
                    {
                        // Fetch summary data
                        var summaryResult = await _context.Set<DrSalesSummary>().FromSqlRaw(
                            "CALL sp_GetSalesByDriver(@sponsorId, @driverId, @startDate, @endDate, @viewType)",
                            new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                            new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                            new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                            new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                            new MySqlParameter("@viewType", viewType)
                        ).ToListAsync();

                        return Ok(summaryResult);
                    }
                    else if (viewType == "detail")
                    {
                        // Fetch detailed data
                        var detailedResult = await _context.Set<SalesDetail>().FromSqlRaw(
                            "CALL sp_GetSalesByDriver(@sponsorId, @driverId, @startDate, @endDate, @viewType)",
                            new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                            new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                            new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                            new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value)
                        ).ToListAsync();

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

        // currently broken rn, dw about it just needs a procedure in db
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

        [HttpPost("export-pdf")]
        public IActionResult ExportPdf([FromBody] ExportRequest request)
        {
            try
            {
                // Validate input
                if (request == null || request.Data == null || !request.Data.Any())
                {
                    return BadRequest("No data provided to export.");
                }

                // Generate PDF using QuestPDF
                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(50);
                        page.DefaultTextStyle(x => x.FontSize(12));
                        
                        // Header Section
                        page.Header().Element(ComposeHeader(request.ReportType, request.Metadata));

                        // Content Section
                        page.Content().Element(ComposeTable(request.Data));

                        // Footer Section
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Generated on ");
                            text.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC").Bold();
                            text.Span(" | Page ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                }).GeneratePdf();

                // Return PDF
                return File(pdfBytes, "application/pdf", $"{request.ReportType}_report.pdf");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating PDF report");
                return StatusCode(500, "Failed to generate PDF report.");
            }
        }

        [HttpPost("export-csv")]
        public IActionResult ExportCsv([FromBody] ExportRequest request)
        {
            try
            {
                // Validate input
                if (request == null || request.Data == null || !request.Data.Any())
                {
                    return BadRequest("No data provided to export.");
                }

                var csvLines = new List<string>();

                // Add Headers
                var headers = string.Join(",", request.Data.First().Keys);
                csvLines.Add(headers);

                // Add Data Rows
                foreach (var row in request.Data)
                {
                    var values = string.Join(",", row.Values.Select(v => v?.ToString()?.Replace(",", ";") ?? "N/A"));
                    csvLines.Add(values);
                }

                // Convert to CSV
                var csvContent = string.Join(Environment.NewLine, csvLines);
                var csvBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

                // Return CSV
                return File(csvBytes, "text/csv", $"{request.ReportType}_report.csv");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating CSV report");
                return StatusCode(500, "Failed to generate CSV report.");
            }
        }

        // helper method for generate pdf
        private static Action<IContainer> ComposeHeader(string reportType, Dictionary<string, string>? metadata)
        {
            return container =>
            {
                container.Column(column =>
                {
                    column.Spacing(10);
                    
                    column.Item().Text($"Report: {reportType.ToUpper()}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    if (metadata != null)
                    {
                        foreach (var item in metadata)
                        {
                            column.Item().Text($"{item.Key}: {item.Value}").FontSize(12);
                        }
                    }
                });
            };
        }

        // helper method for generate pdf
        private static Action<IContainer> ComposeTable(List<Dictionary<string, object>> data)
        {
            return container =>
            {
                var headerStyle = TextStyle.Default.SemiBold();

                container.PaddingVertical(10).Table(table =>
                {
                    // Define table columns
                    var columnCount = data.First().Keys.Count;
                    table.ColumnsDefinition(columns =>
                    {
                        for (int i = 0; i < columnCount; i++)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    // Add headers
                    table.Header(header =>
                    {
                        foreach (var key in data.First().Keys)
                        {
                            header.Cell().Element(cell =>
                            {
                                cell.Text(key)
                                    .Style(headerStyle);
                            });
                        }
                        header.Cell().ColumnSpan((uint)columnCount).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                    });

                    // Add rows
                    foreach (var row in data)
                    {
                        foreach (var value in row.Values)
                        {
                            table.Cell().Element(cell =>
                            {
                                cell.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5)
                                    .Text(value?.ToString() ?? "N/A")
                                    .FontSize(10);
                            });
                        }
                    }
                });
            };
        }

        


    }
}
