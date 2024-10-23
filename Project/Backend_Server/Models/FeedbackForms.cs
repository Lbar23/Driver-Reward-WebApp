using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class FeedbackForms
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Description { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}