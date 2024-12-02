using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class PurchaseProducts
    {
        // Composite Primary Key: (PurchaseID, ProductID)
        public required int PurchaseID { get; set; } // FK to Purchases
        public required int ProductID { get; set; } // FK to Products
        
        // Additional attributes
        public required string PurchasedProductName { get; set; } // Snapshot of product name
        public required decimal PurchasedUnitPrice { get; set; } // Snapshot of currency unit price
        public required int PointsSpent { get; set; }
        public required int Quantity { get; set; }

        // Navigation properties
        public required Products Product { get; set; }
        public required Purchases Purchase { get; set; }
    }

}