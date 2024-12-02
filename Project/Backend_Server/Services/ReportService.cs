using Backend_Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Backend_Server.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Infrastructure;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using MySqlConnector;

namespace Backend_Server.Services;

public class ReportService
{
    private readonly AppDBContext _context;
    private readonly UserManager<Users> _userManager;

    public ReportService(AppDBContext context,
                         UserManager<Users> userManager)
    {
        _context = context;
        _userManager = userManager;
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

    public async Task<List<SpSalesSummary>> GetSponsorSalesSummary(
           int? sponsorId, DateTime? startDate, DateTime? endDate, string viewType="Summary")
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Set<SpSalesSummary>().FromSqlRaw(
                "CALL sp_GetSalesBySponsor(@sponsorId, @startDate, @endDate, @viewType)",
                new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                new MySqlParameter("@viewType", viewType)
            ).ToListAsync();
        });
    }

    public async Task<List<SalesDetail>> GetSponsorSalesDetail(
           int? sponsorId, DateTime? startDate, DateTime? endDate, string viewType="Detail")
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Set<SalesDetail>().FromSqlRaw(
                "CALL sp_GetSalesBySponsor(@sponsorId, @startDate, @endDate, @viewType)",
                new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                new MySqlParameter("@viewType", viewType)
            ).ToListAsync();
        });
    }

    public async Task<List<DrSalesSummary>> GetDriverSalesSummary(
           int? sponsorId, int? driverId, DateTime? startDate, DateTime? endDate, string viewType="Summary")
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Set<DrSalesSummary>().FromSqlRaw(
                "CALL sp_GetSalesByDriver(@sponsorId, @driverId, @startDate, @endDate, @viewType)",
                new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                new MySqlParameter("@viewType", viewType)
            ).ToListAsync();
        });
    }

    public async Task<List<DrSalesSummary>> GetDriverSalesDetail(
           int? sponsorId, int? driverId, DateTime? startDate, DateTime? endDate, string viewType="Detail")
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Set<DrSalesSummary>().FromSqlRaw(
                "CALL sp_GetSalesByDriver(@sponsorId, @driverId, @startDate, @endDate, @viewType)",
                new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value),
                new MySqlParameter("@viewType", viewType)
            ).ToListAsync();
        });
    }

    public async Task<List<InvoiceDetail>> GetInvoiceReport(
           int? sponsorId, DateTime? startDate, DateTime? endDate)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Set<InvoiceDetail>().FromSqlRaw(
                "CALL sp_GetInvoiceReport(@sponsorId, @startDate, @endDate)",
                new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value)
            ).ToListAsync();
        });
    }

    public async Task<List<DriverPoints>> GetDriverPointTracking(
           int? driverId, int? sponsorId, DateTime? startDate, DateTime? endDate)
    {
        return await ExecuteWithRetryAsync(async () =>
        {
            return await _context.Set<DriverPoints>().FromSqlRaw(
                "CALL sp_GetDriverPointTracking(@driverId, @sponsorId, @startDate, @endDate)",
                new MySqlParameter("@driverId", driverId ?? (object)DBNull.Value),
                new MySqlParameter("@sponsorId", sponsorId ?? (object)DBNull.Value),
                new MySqlParameter("@startDate", startDate ?? (object)DBNull.Value),
                new MySqlParameter("@endDate", endDate ?? (object)DBNull.Value)
            ).ToListAsync();
        });
    }

    public byte[] ExportPdf([FromBody] ExportRequest request)
    {
        try
        {
            // Validate input
            if (request == null || request.Data == null || !request.Data.Any())
            {
                throw new Exception("No data provided to export.");
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
            return pdfBytes;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating PDF report");
            throw new Exception("Error generating PDF report");
        }
    }

    [HttpPost("export-csv")]
    public byte[] ExportCsv([FromBody] ExportRequest request)
    {
        try
        {
            // Validate input
            if (request == null || request.Data == null || !request.Data.Any())
            {
                throw new Exception("No data provided to export.");
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
            return csvBytes;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error generating CSV report");
            throw new Exception("Failed to generate CSV report.");
        }
    }

    // helper method to generate pdf
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

    // helper method to generate pdf
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
