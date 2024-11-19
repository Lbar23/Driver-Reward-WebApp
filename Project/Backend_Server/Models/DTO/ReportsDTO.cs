using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


// For better scalability, 
// turn every DTO going forward should be records instead of classes
// (since they are inherently immutable data carriers)

//Fun fact; These were the cause to my predicament, which I never took into effect...

/*
    These should NEVER be Mapped to the DB (like discussed earlier, but anyway); it overtakes any other key namesake from the DB
    And EF Core does NOT like that (AppDBContext)

    Changed them to NotMapped
*/

namespace Backend_Server.Models.DTO
{
    [NotMapped]
    public record SalesSummary(
        string SponsorName,
        decimal TotalSales,
        int TotalDrivers
    );

    [NotMapped]
    public record SalesDetail(
        string SponsorName,
        string DriverName,
        DateTime TransactionDate,
        string ProductName,
        decimal SaleAmount
    );

    [NotMapped]
    public record DriverPoints(
        string DriverName,
        int TotalPoints, // <-- This little bugger in particular
        int PointsChanged,
        DateTime TransactionDate,
        string SponsorName,
        string Reason
    );

    [NotMapped]
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