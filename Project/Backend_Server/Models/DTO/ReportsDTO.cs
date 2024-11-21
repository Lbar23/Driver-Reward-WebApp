using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Server.Models.DTO
{
    [NotMapped]
    public record SalesSummary(
        string SponsorName,
        decimal? TotalSales,
        int? TotalDrivers
    );

    [NotMapped]
    public record SalesDetail(
        string SponsorName,
        string? DriverName,
        DateTime? TransactionDate,
        string? ProductName,
        decimal? SaleAmount
    );

    [NotMapped]
    public record DriverPoints(
        string DriverName,
        int? TotalPoints,
        int PointsChanged,
        DateTime? TransactionDate,
        string SponsorName,
        string Reason
    );

    [NotMapped]
    public record InvoiceDetail(
        string SponsorName,
        string DriverName,
        decimal? TotalPurchaseValue,
        decimal? DriverFee,
        int? PurchaseCount
    );

    [NotMapped]
    public record AuditLog(
        DateTime Timestamp,
        string Category,
        string? DriverName,
        string Description
    );
}
