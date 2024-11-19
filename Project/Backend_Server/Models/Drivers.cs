using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Drivers
    {
        public int UserID { get; set; }
        public string? FirstName { get; set; } //Change these to required later...and updated DTOs and Controllers
        public string? LastName { get; set; } //Ditto
        //public int SponsorID { get; set; }
        //public int TotalPoints { get; set; }
        public ICollection<SponsorDrivers> SponsorDrivers { get; set; } = []; //Added after adding bridge entity for connection
    }
}