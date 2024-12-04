using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Backend_Server.Models.DTO
{
    // DTO for Sponsor details
    [NotMapped]
    public record SponsorDto
    {
        public int SponsorID { get; init; }
        public required string CompanyName { get; init; }
        public decimal PointDollarValue { get; init; }
    }

    // DTO for displaying point value information
    [NotMapped]
    public record PointValueDto
    {
        public int TotalPoints { get; init; }
        public decimal PointValue { get; init; }
        public required string SponsorName { get; init; }
        public decimal TotalValue => TotalPoints * PointValue;
    }

    // DTO for a transaction record
    [NotMapped]
    public record TransactionDto
    {
        public DateTime Date { get; init; }
        public int Points { get; init; }
        public required string Type { get; init; } // e.g., Add or Subtract
        public required string Reason { get; init; }
        public required string SponsorName { get; init; }
        public string? Status { get; init; } // Optional for status like Approved or Pending
    }

    // DTO for submitting a purchase request
    public class PurchaseRequest
    {
        public int SponsorID { get; set; }
        public int UserID { get; set; }
        public int TotalPointsSpent { get; set; }
        public DateTime PurchaseDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<ProductDto> Products { get; set; } = new();
    }

    [NotMapped]
    public record UpdatePurchase
    {
        public List<ProductDto> UpdatedProducts { get; set; } = new();
        public int UpdatedPointsSpent { get; set; }
    }

    [NotMapped]
    public record PurchaseProductDto
    {
        public int ProductID { get; set; }
        public required string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int PointsSpent { get; set; }
        public int Quantity { get; set; }
    }

    // DTO for listing driver information
    [NotMapped]
    public record DriverListDto
    {
        public int UserID { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public int TotalPoints { get; init; }
    }


    // DTO for changing user type
    [NotMapped]
    public record ChangeUserTypeDto
    {
        public int UserId { get; set; }
        public int SponsorID { get; set; }
        public Sponsors? Sponsor { get; set; }
        public required string NewUserType { get; set; }
    }
    
    /// <summary>
    /// DTO representing the vw_AllAdmins database view
    /// Maps admin user information including role and login details
    /// </summary>
    [NotMapped]
    public record ViewAdminsDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    /// <summary>
    /// DTO representing the vw_AllDrivers database view
    /// Maps driver information including their sponsor and point details
    /// </summary>
    [NotMapped]
    public record ViewDriversDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int SponsorID { get; set; }
        public string SponsorName { get; set; } = string.Empty;
        public int DriverPoints { get; set; }
        public int MilestoneLevel { get; set; }
    }

    /// <summary>
    /// DTO representing the vw_AllSponsorUsers database view
    /// Maps sponsor user information including company and role details
    /// </summary>
    /// 
    [NotMapped]
    public record ViewSponsorUsersDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int SponsorID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string SponsorType { get; set; } = string.Empty;
        public decimal PointDollarValue { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime JoinDate { get; set; }
    }

    [NotMapped]
    public record InitResetPasswordDto
    {
        public required string Email { get; set; }
    }

    // DTO for creating a new user
    [NotMapped]
    public record CreateUserDto
    {

        // For all users (required)
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? Password { get; set; }
        public required string State { get; set; }
        public required string Role { get; set; } // Role to assign replaces usertype
        public string? SponsorName { get; set; }
        public required bool Enable2FA { get; set; } 
        public NotificationPref NotifyPref { get; set; } = NotificationPref.Email; // Updated

        // Optional fields for role-specific actions
        public int? SponsorID { get; set; } // Required only for SponsorUsers or DriverUsers
        public bool? IsPrimary { get; set; } = false; // Default for sponsor users
        public decimal? DriverPointValue { get; set; } // only required for drivers

        public IEnumerable<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required");

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email is required");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required");

            if (string.IsNullOrWhiteSpace(State) || State.Length != 2)
                errors.Add("State is required and must be a valid two-character abbreviation.");

            if (string.IsNullOrWhiteSpace(Role))
                errors.Add("Role is required.");

            // Role-specific validations
            if (Role.Equals("Sponsor", StringComparison.OrdinalIgnoreCase) || 
                Role.Equals("Driver", StringComparison.OrdinalIgnoreCase))
            {
                if (!SponsorID.HasValue)
                    errors.Add("SponsorID is required for Sponsor or Driver users.");
            }

            return errors;
        }
    }

    // DTO for user login
    [NotMapped]
    public record UserLoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
    
    // DTO for two-factor authentication
    [NotMapped]
    public record TwoFactorDto
    {
        public string UserId { get; set; } // Updated to match database
        public string Code { get; set; }
    }
    
    // DTO for resetting password
    [NotMapped]
    public record ResetPasswordDto 
    {
        public required int UserId { get; set; }
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

    // DTO for changing password
    [NotMapped]
    public record ChangePasswordDto
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }


    public class ProductDto
    {
        public int SponsorID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public decimal CurrencyPrice { get; set; }
        public int PriceInPoints { get; set; }
        public string ExternalID { get; set; }
        public string ImageUrl { get; set; }
        public bool Availability { get; set; }
    }

}
