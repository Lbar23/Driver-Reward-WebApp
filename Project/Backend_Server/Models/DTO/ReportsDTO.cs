using System.ComponentModel.DataAnnotations.Schema;

namespace Backend_Server.Models.DTO
{
    [NotMapped]
    public record ExportRequest
    {
        public string ReportType { get; set; } = "report";
        public List<Dictionary<string, object>> Data { get; set; } = new();
        public Dictionary<string, string>? Metadata { get; set; }
    }
    [NotMapped]
    public record DrSalesSummary(
        string? DriverName,        
        string? SponsorName,       
        decimal TotalSales,       
        int? PurchaseCount        
    );

    [NotMapped]
    public record SpSalesSummary(
        string? SponsorName,       
        decimal TotalSales,       
        int? TotalDrivers         
    );

    [NotMapped]
    public record DrSalesDetail(
        string? DriverName,      
        string? SponsorName,       
        DateTime TransactionDate,  
        string? ProductName,       
        decimal SaleAmount       
    );

      [NotMapped]
    public record SpSalesDetail(
        string? SponsorName,       
        DateTime TransactionDate,  
        decimal SaleAmount,   
        string? ProductName    
    );

    [NotMapped]
    public record DriverPoints(
        string? DriverName,
        int? TotalPoints,
        int PointsChanged,
        DateTime? TransactionDate,
        string SponsorName,
        string Reason
    );

    [NotMapped]
    public record InvoiceDetail(
        string? SponsorName,
        string? DriverName,
        decimal TotalPurchaseValue,
        decimal DriverFee,
        int? PurchaseCount
    );

    [NotMapped]
    public record AuditLogReport(
        DateTime Timestamp,
        string Category,
        string? DriverName,
        string Description
    );
}
