using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Products
    {
        public required int SponsorID { get; set; }
        public int ProductID { get; set; }
        public required string ProductName { get; set; }
        public required string Category { get; set; }
        public required string Description { get; set; } = string.Empty;
        public required decimal CurrencyPrice {get; set; }
        public required int PriceInPoints { get; set; }
        public required string ExternalID { get; set; }
        public required string ImageUrl {get; set; }
        public bool Availability { get; set; }
    }
}