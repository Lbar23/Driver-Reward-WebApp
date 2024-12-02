using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum FeedbackType
    {
        Suggestion,
        BugReport,
        Complaint,
        Compliment,
        Inquiry,
        FeatureRequest,
        General
    }

    public class FeedbackForms
    {
        public int FeedbackID { get; set; } // Primary Key
        public required string FirstName { get; set; }
        public required string Email { get; set; }
        public required FeedbackType FeedbackCategory { get; set; }
        public required string Comments { get; set; }
        public required DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    }
}
