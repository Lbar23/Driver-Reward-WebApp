using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


// For better scalability, 
// turn every DTO going forward should be records instead of classes
// (since they are inherently immutable data carriers)

namespace Backend_Server.Models.DTO
{
    [Keyless]
    public record SalesSummary(
        string SponsorName,
        decimal TotalSales,
        int TotalDrivers
    );

    [Keyless]
    public record SalesDetail(
        string SponsorName,
        string DriverName,
        DateTime TransactionDate,
        string ProductName,
        decimal SaleAmount
    );

    [Keyless]
    public record DriverPoints(
        string DriverName,
        int TotalPoints,
        int PointsChanged,
        DateTime TransactionDate,
        string SponsorName,
        string Reason
    );

    [Keyless]
    public record InvoiceDetail(
        string SponsorName,
        string DriverName,
        decimal TotalPurchaseValue,
        decimal DriverFee,
        int PurchaseCount
    );

    [NotMapped]
    public record AuditLog(
        DateTime Timestamp,
        string Category,
        string DriverName,
        string Description
    );
}