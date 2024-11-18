using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


// For better scalability, 
// turn every DTO going forward should be records instead of classes
// (since they are inherently immutable data carriers)

namespace Backend_Server.Models.DTO
{
    [NotMapped]
    public record SponsorDto //mainly here so Drivers can switch between different sponsors
    {
        public int SponsorID { get; init; }
        public required string CompanyName { get; init; }
        public decimal PointDollarValue { get; init; }
    }

    [NotMapped]
    public record PointValueDto
    {
        public int TotalPoints { get; init; }
        public decimal PointValue { get; init; }
        public required string SponsorName { get; init; }
        public decimal TotalValue => TotalPoints * PointValue;
    }

    [NotMapped]
    public record TransactionDto
    {
        public DateTime Date { get; init; }
        public int Points { get; init; }
        public required string Type { get; init; }
        public required string Reason { get; init; }
        public required string SponsorName { get; init; }
        public string? Status { get; init; }
    }

    [NotMapped]
    public record DriverListDto
    {
        public int UserID { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public int TotalPoints { get; init; }
        // public string? City { get; init; }
        // public string? State { get; init; }
    }

    [NotMapped]
    public record UserRegisterDto 
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool Enable2FA { get; set; } 
    }

    [NotMapped]
    public record ChangeUserTypeDto
    {
        public int UserId { get; set; }
        public required string NewUserType { get; set; }
    }

    [NotMapped]
    public record CreateUserDto
    {
        //For all users; Required
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required UserType UserType { get; set; }
        public required string Password { get; set; }

        // For sponsors; So make them conditional
        public string? CompanyName { get; set; }
        public string? SponsorType { get; set; }
        public decimal? PointDollarValue { get; set; }

        // For drivers; So make them conditional
        public int? SponsorID { get; set; }

        public IEnumerable<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Username))
                errors.Add("Username is required");

            if (string.IsNullOrWhiteSpace(Email))
                errors.Add("Email is required");

            if (string.IsNullOrWhiteSpace(Password))
                errors.Add("Password is required");

            switch (UserType)
            {
                case UserType.Sponsor when string.IsNullOrWhiteSpace(CompanyName):
                    errors.Add("Company name is required for sponsors");
                    break;
                case UserType.Sponsor when string.IsNullOrWhiteSpace(SponsorType):
                    errors.Add("Sponsor type is required for sponsors");
                    break;
                case UserType.Driver when !SponsorID.HasValue:
                    errors.Add("Sponsor ID is required for drivers");
                    break;
            }

            return errors;
        }
    };

    [NotMapped]
    public record UserLoginDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
    
    [NotMapped]
    public record TwoFactorDto
    {
        public required string UserId { get; set; }
        public required string Code { get; set; }
    }
    
    [NotMapped]
    public record ResetPasswordDto 
    {
        public required string NewPassword { get; set; }
    }

    [NotMapped]
    public record ChangePasswordDto
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}