using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum ProcessedReason
    {
        // Accepted Reason
        MeetsCriteria,              // default accepted reason
        Referral,                   // approved b/c referral

        // Rejected Reasons
        InvalidApplication,         // Application missing required details or documents
        Ineligible,                 // Driver failed the sponsor's eligibility criteria
        FullCapacity,               // Program already reached its capacity
    }
    
    public enum AppStatus
    {
        Submitted, 
        Approved,
        Rejected,
    }

    public class DriverApplications
    {
        public int ApplicationID { get; set; } // Primary Key
        public required int SponsorID { get; set; } // FK to SponsorDrivers
        public required int UserID { get; set; }  // FK to SponsorDrivers
        public required AppStatus Status { get; set; } = AppStatus.Submitted;
        public required DateOnly ApplyDate { get; set; }
        public required DateTime LastModified { get; set; }
        public DateOnly? ProcessedDate { get; set; }
        public ProcessedReason? ProcessReason { get; set; } // Optional only because the initial state is submitted
        public string? Comments { get; set; } = string.Empty; // Optional comments

        // Navigation Properties
        public required SponsorDrivers SponsorDriver { get; set; }
    }
}