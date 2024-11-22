using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum SponsorRole
    {
        Standard,
        Manager,
        Admin
    }

    public class SponsorUsers
    {
        public int UserID { get; set; }
        public int SponsorID { get; set; }
        public bool IsPrimarySponsor { get; set; } = false; //easier identifier for main sponsor under company name
        public DateTime JoinDate { get; set; }
        public SponsorRole SponsorRole { get; set; } = SponsorRole.Standard;
        public required Users User { get; set; }
        public required Sponsors Sponsor { get; set; }
    }
}