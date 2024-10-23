using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum OrderStatus
    {
    Ordered, 
    Cancelled,
    Refunded,
    }
    public class Purchases
    {
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public int PurchaseID { get; set; }
        public int PointsSpent { get; set; }
        public DateTime PurchaseDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Ordered;
        public required Drivers Driver { get; set; }
        public required Products Product { get; set; }
    }
}