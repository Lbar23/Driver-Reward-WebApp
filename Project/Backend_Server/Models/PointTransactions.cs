using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{

    public enum ActionType
    {
        Manual, 
        Automatic
    }

    public enum PointChangeReason
    {
        DriverPurchase, 
        SponsorPurchase,
        // Positive Reasons (Add Points)
        SafeDriving,                // Demonstrating safe driving habits
        CustomerFeedback,           // Positive feedback received from customers
        ExtraEffort,                // Going above and beyond expectations
        NoIncidents,                // Operating incident-free over a defined period
        ProgramParticipation,       // Participating in sponsor-led programs or training sessions
        MilestoneAchievement,       // Achieving specific milestones, such as delivery count
        ReferralBonus,              // Referring other drivers to the program

        // Negative Reasons (Subtract Points)
        UnsafeDriving,              // Demonstrating unsafe driving habits
        LeftSponsor,                // For when drivers change sponsors
        PropertyDamage,              // Causing damage to the vehicle or goods
        CustomerComplaints,         // Negative feedback received from customers
        ProgramViolations,          // Violating terms of the incentive program
        IncidentReport             // Being involved in an incident or accident
    }

    public class PointTransactions
    {
        public required int SponsorID { get; set; }               // FK for the Sponsor
        public required int UserID { get; set; }                // FK for the Driver
        public int TransactionID { get; set; }                    // Primary Key
        public required int PointsChanged { get; set; }           // Net change in points
        public required PointChangeReason Reason { get; set; }    // Reason for the transaction
        public required ActionType ActionType { get; set; }       // Manual or Automatic
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;  // Transaction timestamp

        // Relationships
        public required SponsorDrivers SponsorDriver { get; set; }  // FK to SponsorDrivers
    }
}