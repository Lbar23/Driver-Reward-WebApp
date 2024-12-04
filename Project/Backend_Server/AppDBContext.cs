using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Backend_Server.Models.DTO;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Backend_Server
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : IdentityDbContext<Users, IdentityRole<int>, int>(options)
    {
        /// <summary>
        /// Core User Management
        /// Tables managing users, roles, and relationships between sponsors and drivers.
        /// </summary>
        public required DbSet<Sponsors> Sponsors { get; set; }
        public required DbSet<SponsorUsers> SponsorUsers { get; set; }
        public required DbSet<SponsorDrivers> SponsorDrivers { get; set; }

        /// <summary>
        /// System Features
        /// Tables enabling core functionalities and additional application features.
        /// </summary>
        public required DbSet<About> About { get; set; }
        public required DbSet<Products> Products { get; set; }
        public required DbSet<NotifyTypes> NotifyTypes { get; set; }
        public required DbSet<DriverApplications> DriverApplications { get; set; }
        public required DbSet<FeedbackForms> FeedbackForms { get; set; }

        /// <summary>
        /// Web Transaction Logging
        /// Tables for logging transactional data and events for accountability and auditing.
        /// </summary>
        public required DbSet<Purchases> Purchases { get; set; }
        public required DbSet<PurchaseProducts> PurchaseProducts { get; set; }
        public required DbSet<PointTransactions> PointTransactions { get; set; }
        public required DbSet<AuditLogs> AuditLogs { get; set; }
        public required DbSet<NotificationHistory> NotificationHistory { get; set; }
        public required DbSet<AccountActivity> AccountActivity { get; set; }
        public required DbSet<Authentications> Authentications { get; set; }

        /// <summary>
        /// Reporting and Summaries
        /// Tables supporting reporting and aggregated data for sponsors and administrators.
        /// </summary>

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("AspNetUsers");
                 // User-to-Role FK

                // Mapping RoleID as a Foreign Key to IdentityRole<int>.
                entity.HasOne(e => e.Role)
                    .WithMany()
                    .HasForeignKey(e => e.RoleID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                // Configure State property to ensure it's abbreviated
                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(2) // Limit to 2 characters
                    .IsFixedLength() // Ensure it's always 2 characters long
                    .HasComment("Two-character state abbreviation (ex. SC for South Carolina)");

                entity.Property(e => e.ProfileImgUrl)
                    .HasMaxLength(2048); // Allowing URLs up to 2048 characters.

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.LastLogin)
                    .HasColumnType("DATETIME");

                entity.Property(e => e.NotifyPref)
                    .HasConversion<string>() // Storing enum as string for readability.
                    .HasDefaultValue(NotificationPref.None);

                // Email and Username constraints from Identity.
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            /// <summary>
            /// Represents sponsors who manage drivers and provide point-based incentives.
            /// Includes relationships with SponsorDrivers and SponsorUsers.
            /// </summary>
            modelBuilder.Entity<Sponsors>(entity =>
            {
                entity.ToTable("Sponsors");

                // Primary Key
                entity.HasKey(e => e.SponsorID);

                // SponsorType Enum Conversion
                entity.Property(e => e.SponsorType)
                    .IsRequired()
                    .HasConversion<string>() // Store as string for readability in DB
                    .HasMaxLength(20) // Enforce a reasonable length for enum string
                    .HasComment("Defines the type of sponsor (Logistics, Trucking, etc.)");

                // Company Name
                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasComment("Name of the sponsor company");

                // Point-to-Dollar Value
                entity.Property(e => e.PointDollarValue)
                    .IsRequired()
                    .HasPrecision(10, 2) // 10 digits, 2 decimals for monetary precision
                    .HasDefaultValue(0.01m)
                    .HasComment("Dollar value of one point, default is 0.01 USD");

                entity.Property(e => e.MilestoneThreshold)
                      .IsRequired()
                      .HasDefaultValue(0)
                      .HasComment("If milestones aren't enabled the value is 0");

                // Relationships to SponsorDrivers and SponsorUsers
                entity.HasMany<SponsorDrivers>()
                    .WithOne(sd => sd.Sponsor)
                    .HasForeignKey(sd => sd.SponsorID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<SponsorUsers>()
                    .WithOne(su => su.Sponsor)
                    .HasForeignKey(su => su.SponsorID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            /// <summary>
            /// Represents the relationship between sponsors and drivers,
            /// including sponsor-specific milestones and points.
            /// </summary>
            modelBuilder.Entity<SponsorDrivers>(entity =>
            {
                entity.ToTable("SponsorDrivers"); // Table name

                // Composite Primary Key
                entity.HasKey(sd => new { sd.SponsorID, sd.UserID });

                // Foreign Key: UserID references Users table
                entity.HasOne(sd => sd.User)
                    .WithMany(d => d.SponsorDrivers)
                    .HasForeignKey(sd => sd.UserID)
                    .OnDelete(DeleteBehavior.Cascade) // Cascade delete if a driver is deleted
                    .HasConstraintName("FK_SponsorDrivers_Driver");

                // Foreign Key: SponsorID references Sponsors table
                entity.HasOne(sd => sd.Sponsor)
                    .WithMany(s => s.SponsorDrivers)
                    .HasForeignKey(sd => sd.SponsorID)
                    .OnDelete(DeleteBehavior.Cascade) // Cascade delete if a sponsor is deleted
                    .HasConstraintName("FK_SponsorDrivers_Sponsor");

                // Points: Required with a default value of 0
                entity.Property(sd => sd.Points)
                    .IsRequired()
                    .HasDefaultValue(0)
                    .HasComment("Points specific to this sponsor");

                // MilestoneLevel: Optional, tracks performance milestones
                entity.Property(sd => sd.MilestoneLevel)
                    .HasComment("Optional milestone level for tracking performance");

                // DriverPointValue: Required, tracks point value for this specific driver-sponsor relationship
                entity.Property(sd => sd.DriverPointValue)
                    .IsRequired()
                    .HasDefaultValue(1) // Set a reasonable default value if needed
                    .HasComment("Custom point value for driver under this sponsor");

                // Additional Index
                entity.HasIndex(sd => new { sd.UserID, sd.SponsorID })
                    .HasDatabaseName("IX_SponsorDrivers_DriverSponsor");
            });

            /// <summary>
            /// Represents the relationship between sponsors and users,
            /// including information about primary sponsorship and join dates.
            /// </summary>
            modelBuilder.Entity<SponsorUsers>(entity =>
            {
                entity.ToTable("SponsorUsers"); // Table name

                // Composite Primary Key
                entity.HasKey(su => new { su.UserID, su.SponsorID });

                // Foreign Key: UserID references Users table
                entity.HasOne(su => su.User)
                    .WithMany(u => u.SponsorUsers)
                    .HasForeignKey(su => su.UserID)
                    .OnDelete(DeleteBehavior.Cascade) // Cascade delete if a user is deleted
                    .HasConstraintName("FK_SponsorUsers_User");

                // Foreign Key: SponsorID references Sponsors table
                entity.HasOne(su => su.Sponsor)
                    .WithMany(s => s.SponsorUsers)
                    .HasForeignKey(su => su.SponsorID)
                    .OnDelete(DeleteBehavior.Cascade) // Cascade delete if a sponsor is deleted
                    .HasConstraintName("FK_SponsorUsers_Sponsor");

                // IsPrimary: Required with default value of false
                entity.Property(su => su.IsPrimary)
                    .IsRequired()
                    .HasDefaultValue(false)
                    .HasComment("Indicates if this user is the primary sponsor representative.");

                // JoinDate: Required
                entity.Property(su => su.JoinDate)
                    .IsRequired()
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasComment("Date the user joined the sponsor organization.");

                // Additional Index
                entity.HasIndex(su => new { su.SponsorID, su.IsPrimary })
                    .HasDatabaseName("IX_SponsorUsers_SponsorPrimary");
            });

            /// <summary>
            /// Stores application metadata such as release version, product name, and description.
            /// Tracks project development and updates.
            /// </summary>
            modelBuilder.Entity<About>(entity =>
            {
                entity.ToTable("About");

                // Primary Key
                entity.HasKey(e => e.Release);

                // Columns
                entity.Property(e => e.Release)
                    .HasDefaultValueSql("(CURRENT_DATE)")
                    .HasColumnType("DATE")
                    .HasComment("Release date of the current version of the program.");

                entity.Property(e => e.Team)
                    .IsRequired()
                    .HasDefaultValue(16)
                    .ValueGeneratedNever() // Make `Team` immutable
                    .HasComment("Team number responsible for the project. Immutable.");

                entity.Property(e => e.Version)
                    .IsRequired()
                    .HasComment("Current version of the program.");

                entity.Property(e => e.Product)
                    .IsRequired()
                    .HasComment("Name of the program.");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasComment("Description of the program.");
            });

            /// <summary>
            /// Represents products available in sponsor catalogs for driver redemption.
            /// Includes pricing, categories, and external API references.
            /// </summary>
            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("Products");

                // Primary Key
                entity.HasKey(e => e.ProductID);

                // Relationships
                entity.HasOne<Sponsors>()
                    .WithMany()
                    .HasForeignKey(p => p.SponsorID)
                    .HasConstraintName("FK_Products_Sponsors")
                    .OnDelete(DeleteBehavior.Cascade);

                // Properties
                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasComment("The name of the product.");

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasComment("The category the product belongs to.");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .HasDefaultValue(string.Empty)
                    .HasComment("Parsed description of the product.");

                entity.Property(e => e.CurrencyPrice)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasComment("The monetary price of the product.");

                entity.Property(e => e.PriceInPoints)
                    .IsRequired()
                    .HasComment("The equivalent price of the product in points.");

                entity.Property(e => e.ExternalID)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("External ID used to track the product from the external source, eBay API.");

                entity.Property(e => e.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasComment("The URL for the product's primary image.");

                entity.Property(e => e.Availability)
                    .HasDefaultValue(true)
                    .HasComment("Indicates whether the product is currently available for purchase.");
            });

            /// <summary>
            /// Defines notification templates and their required variables for dynamic messaging.
            /// Used for sending email and SMS notifications.
            /// </summary>
            modelBuilder.Entity<NotifyTypes>(entity =>
            {
                entity.ToTable("NotifyTypes");

                // Primary Key
                entity.HasKey(nt => nt.TypeID);

                // Enum: NotifyCategory
                entity.Property(nt => nt.Category)
                    .IsRequired()
                    .HasConversion<string>() // Store enum as string for readability
                    .HasMaxLength(50)
                    .HasComment("Category of notification, such as TwoFA, Purchase, PointsChange, etc.");

                // Description
                entity.Property(nt => nt.Description)
                    .HasMaxLength(255)
                    .HasComment("Optional description of the notification type.");

                // Email Template ID
                entity.Property(nt => nt.EmailTemplateID)
                    .HasMaxLength(100)
                    .HasComment("SendGrid template ID for email notifications.");

                // Template Fields
                entity.Property(nt => nt.TemplateFieldsJson)
                    .HasColumnType("VARCHAR(1000)") // Store JSON array of expected fields
                    .HasDefaultValue("[]")
                    .HasComment("JSON array defining expected dynamic fields for templates.");

                // IsActive
                entity.Property(nt => nt.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true)
                    .HasComment("Indicates if the notification type is currently active.");

                // Indexes
                entity.HasIndex(nt => nt.Category)
                    .HasDatabaseName("IX_NotifyTypes_Category");
            });

            /// <summary>
            /// Tracks purchases made by drivers using points.
            /// Connected to PurchaseProducts for multi-product purchases.
            /// </summary>
            modelBuilder.Entity<Purchases>(entity =>
            {
                entity.ToTable("Purchases");

                // Primary Key
                entity.HasKey(p => p.PurchaseID);

                // Required Properties
                entity.Property(p => p.TotalPointsSpent)
                    .IsRequired();

                entity.Property(p => p.PurchaseDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP")
                    .IsRequired();

                entity.Property(p => p.Status)
                    .HasConversion<string>() // Storing enum as string
                    .IsRequired();

                // Foreign Key to SponsorDrivers
                entity.HasOne(p => p.SponsorDriver)
                    .WithMany(sd => sd.Purchases)
                    .HasForeignKey(p => new { p.SponsorID, p.UserID })
                    .OnDelete(DeleteBehavior.Cascade);

                // Optional: Indexes for quick lookup
                entity.HasIndex(p => p.PurchaseDate);
                entity.HasIndex(p => new { p.SponsorID, p.UserID });
            });

            /// <summary>
            /// Tracks individual products within a purchase, storing snapshots of product details.
            /// Represents a many-to-many relationship between Purchases and Products.
            /// </summary>
            modelBuilder.Entity<PurchaseProducts>(entity =>
            {
                entity.ToTable("PurchaseProducts");

                entity.HasKey(pp => new { pp.PurchaseID, pp.ProductID }); // Composite Key

                entity.Property(pp => pp.PurchasedProductName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(pp => pp.PurchasedUnitPrice)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(pp => pp.PointsSpent)
                    .IsRequired();

                entity.Property(pp => pp.Quantity)
                    .IsRequired();

                entity.HasOne(pp => pp.Purchase)
                    .WithMany(p => p.PurchaseProducts)
                    .HasForeignKey(pp => pp.PurchaseID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pp => pp.Product)
                    .WithMany()
                    .HasForeignKey(pp => pp.ProductID)
                    .OnDelete(DeleteBehavior.Restrict); // Preserve product info for snapshot
            });

            /// <summary>
            /// Tracks point transactions for drivers, including reasons and timestamps.
            /// Connected to SponsorDrivers for sponsor-specific point changes.
            /// </summary>
            modelBuilder.Entity<PointTransactions>(entity =>
            {
                entity.ToTable("PointTransactions");

                entity.HasKey(e => e.TransactionID);

                // Relationships
                entity.HasOne(pt => pt.SponsorDriver)
                    .WithMany()
                    .HasForeignKey(e => new { e.SponsorID, e.UserID })  // Composite FK to SponsorDrivers
                    .OnDelete(DeleteBehavior.Cascade);

                // Properties
                entity.Property(e => e.PointsChanged)
                    .IsRequired();

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasConversion<string>()  // Store as string
                    .HasMaxLength(50);

                entity.Property(e => e.ActionType)
                    .IsRequired()
                    .HasConversion<string>()  // Store as string
                    .HasMaxLength(20);

                entity.Property(e => e.TransactionDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");

                // Indexes
                entity.HasIndex(e => new { e.SponsorID, e.UserID })
                      .HasDatabaseName("IX_PointTransactions_SponsorDriver");
                entity.HasIndex(e => e.TransactionDate);
            });

            /// <summary>
            /// Tracks notification history for users, including timestamps, message templates, and statuses.
            /// </summary>
            modelBuilder.Entity<NotificationHistory>(entity =>
            {
                entity.ToTable("NotificationHistory");

                // Primary Key
                entity.HasKey(nh => nh.InstanceID);

                // Foreign Key: NotifyTypeID references NotifyTypes
                entity.HasOne(nh => nh.NotifyTypes)
                    .WithMany()
                    .HasForeignKey(nh => nh.NotifyTypeID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NotificationHistory_NotifyTypes");

                // Foreign Key: UserID references Users
                entity.HasOne(nh => nh.User)
                    .WithMany()
                    .HasForeignKey(nh => nh.UserID)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_NotificationHistory_Users");

                // Properties
                entity.Property(nh => nh.Success)
                    .IsRequired()
                    .HasDefaultValue(false)
                    .HasComment("Indicates whether the notification was successfully sent (true) or failed (false).");

                entity.Property(nh => nh.NotifyDate)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP")
                    .HasComment("Timestamp of when the notification was attempted.");

                // Indexes for optimized queries
                entity.HasIndex(nh => nh.UserID)
                    .HasDatabaseName("IX_NotificationHistory_UserID");

                entity.HasIndex(nh => nh.NotifyTypeID)
                    .HasDatabaseName("IX_NotificationHistory_NotifyTypeID");

                entity.HasIndex(nh => nh.NotifyDate)
                    .HasDatabaseName("IX_NotificationHistory_NotifyDate");
            });

            /// <summary>
            /// Tracks driver applications submitted to sponsors.
            /// Includes status, processing dates, and reasons for acceptance or rejection.
            /// </summary>
            modelBuilder.Entity<DriverApplications>(entity =>
            {
                entity.ToTable("DriverApplications");

                // Primary Key
                entity.HasKey(e => e.ApplicationID);

                // Relationships
                entity.HasOne(e => e.SponsorDriver)
                    .WithMany()
                    .HasForeignKey(e => new { e.SponsorID, e.UserID })
                    .OnDelete(DeleteBehavior.Cascade);

                // Properties
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<string>() // Store enum as string
                    .HasMaxLength(20);

                entity.Property(e => e.ApplyDate)
                    .IsRequired()
                    .HasColumnType("DATE");

                entity.Property(e => e.ProcessedDate)
                    .HasColumnType("DATE");

                entity.Property(e => e.ProcessReason)
                    .HasConversion<string>() // Store enum as string
                    .HasMaxLength(50);

                entity.Property(e => e.Comments)
                    .HasMaxLength(500);

                // Indexes
                entity.HasIndex(e => new { e.SponsorID, e.UserID }); // Optimize composite key lookups
                entity.HasIndex(e => e.ApplyDate); // Optimize filtering by date
                entity.HasIndex(e => e.Status); // Optimize status-based queries
            });

            /// <summary>
            /// Logs key system events such as user actions, point changes, and application statuses.
            /// Includes category, action, and additional context details.
            /// </summary>
            modelBuilder.Entity<AuditLogs>(entity =>
            {
                entity.ToTable("AuditLogs");

                entity.HasKey(e => e.LogID);

                // Relationships
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Properties
                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(e => e.Action)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(e => e.Timestamp)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");

                entity.Property(e => e.AdditionalDetails)
                    .HasColumnType("VARCHAR(1000)") // Store JSON for additional context
                    .HasDefaultValue(string.Empty);

                // Indexes
                entity.HasIndex(e => e.UserID);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Timestamp);
            });

            /// <summary>
            /// Stores feedback submitted by users, categorized by type (e.g., suggestion, complaint).
            /// Includes submission dates and user-provided comments.
            /// </summary>
            modelBuilder.Entity<FeedbackForms>(entity =>
            {
                entity.ToTable("FeedbackForms");

                entity.HasKey(f => f.FeedbackID);

                // Properties
                entity.Property(f => f.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(f => f.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(f => f.FeedbackCategory)
                    .IsRequired()
                    .HasConversion<string>() // Store as string in the database
                    .HasMaxLength(50);

                entity.Property(f => f.Comments)
                    .IsRequired()
                    .HasColumnType("VARCHAR(1000)"); 

                entity.Property(f => f.SubmissionDate)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");

                // Indexes for quick lookups
                entity.HasIndex(f => f.Email);
                entity.HasIndex(f => f.FeedbackCategory);
                entity.HasIndex(f => f.SubmissionDate);
            });


            /// <summary>
            /// Stores user account activity, including timestamps, types, and details.
            /// </summary>
            modelBuilder.Entity<AccountActivity>(entity => 
            {
                entity.ToTable("AccountActivity");

                entity.HasKey(a => a.ActivityId);

                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(a => a.UserId)
                    .IsRequired();

                entity.Property(a => a.Timestamp)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");

                entity.Property(a => a.ActivityType)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);
                
                entity.Property(a => a.Details)
                    .HasMaxLength(500);
                
            });

            /// <summary>
            /// Stores user authentication events, including timestamps, types, and details.
            /// </summary>
            modelBuilder.Entity<Authentications>(entity => 
            {
                entity.ToTable("Authentications");

                entity.HasKey(a => a.AuthID);

                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(a => a.UserID)
                    .IsRequired();

                entity.Property(a => a.Timestamp)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");
                
                entity.Property(a => a.AuthType)
                    .IsRequired()
                    .HasConversion<string>()
                    .HasMaxLength(50);
                
                entity.Property(a => a.Success)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(a => a.UserAgent)
                    .HasMaxLength(60);
                
                entity.Property(a => a.Details)
                    .HasMaxLength(500);
            });


            // For sp_GetSalesBySponsor (both summary and detail use same proc with different viewType)
            modelBuilder.Entity<SpSalesSummary>()
                .HasNoKey()
                .ToFunction("sp_GetSalesSponsorSummary");

            modelBuilder.Entity<SpSalesDetail>()
                .HasNoKey()
                .ToFunction("sp_GetSalesSponsorDetail");

            // For sp_GetSalesByDriver
            modelBuilder.Entity<DrSalesSummary>()
                .HasNoKey()
                .ToFunction("sp_GetSalesByDriver");

                modelBuilder.Entity<DrSalesDetail>()
                .HasNoKey()
                .ToFunction("sp_GetSalesDriverDetail");

            // For sp_GetInvoiceReport
            modelBuilder.Entity<InvoiceDetail>()
                .HasNoKey()
                .ToFunction("sp_GetInvoiceReport");

            // For sp_GetDriverPointTracking
            modelBuilder.Entity<DriverPoints>()
                .HasNoKey()
                .ToFunction("sp_GetDriverPointTracking");

            modelBuilder.Entity<ViewAdminsDto>()
                .HasNoKey()
                .ToView("vw_Admins");

            modelBuilder.Entity<ViewDriversDto>()
                .HasNoKey()
                .ToView("vw_Drivers");

            modelBuilder.Entity<ViewSponsorUsersDto>()
                .HasNoKey()
                .ToView("vw_SponsorUsers");

            modelBuilder.Entity<About>().HasData(
                Enumerable.Range(1, 11).Select(version => new About
                {
                    Release = new DateOnly(2024, 9, 9).AddDays((version - 1) * 7), // Increment by 7 days for each version
                    Team = 16,
                    Version = version,
                    Product = "GitGud Drivers",
                    Description = "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points."
                }).ToArray()
            );


            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "Sponsor", NormalizedName = "SPONSOR" },
                new IdentityRole<int> { Id = 3, Name = "Driver", NormalizedName = "DRIVER" },
                new IdentityRole<int> { Id = 4, Name = "Guest", NormalizedName = "GUEST" }
            );

            modelBuilder.Entity<IdentityRoleClaim<int>>().HasData(
                new IdentityRoleClaim<int> { Id = 1, RoleId = 1, ClaimType = "Permission", ClaimValue = "ManageUsers" },
                new IdentityRoleClaim<int> { Id = 2, RoleId = 1, ClaimType = "Permission", ClaimValue = "CanImpersonate" },
                new IdentityRoleClaim<int> { Id = 3, RoleId = 1, ClaimType = "Permission", ClaimValue = "ViewReports" },
                new IdentityRoleClaim<int> { Id = 4, RoleId = 1, ClaimType = "Permission", ClaimValue = "ViewAuditLogs" },
                new IdentityRoleClaim<int> { Id = 5, RoleId = 2, ClaimType = "Permission", ClaimValue = "ManageCatalog" },
                new IdentityRoleClaim<int> { Id = 6, RoleId = 2, ClaimType = "Permission", ClaimValue = "ManageApplications" },
                new IdentityRoleClaim<int> { Id = 7, RoleId = 2, ClaimType = "Permission", ClaimValue = "ManageDrivers" },
                new IdentityRoleClaim<int> { Id = 8, RoleId = 2, ClaimType = "Permission", ClaimValue = "CanImpersonate" },
                new IdentityRoleClaim<int> { Id = 9, RoleId = 2, ClaimType = "Permission", ClaimValue = "ViewReports" },
                new IdentityRoleClaim<int> { Id = 10, RoleId = 2, ClaimType = "Permission", ClaimValue = "ViewAuditLogs" },
                new IdentityRoleClaim<int> { Id = 11, RoleId = 2, ClaimType = "Permission", ClaimValue = "RedeemPoints" },
                new IdentityRoleClaim<int> { Id = 12, RoleId = 3, ClaimType = "Permission", ClaimValue = "RedeemPoints" }
            );

            modelBuilder.Entity<NotifyTypes>().HasData(
                new NotifyTypes{
                    TypeID = 1,
                    Category = NotifyCategory.Auth,
                    Description = "Authentication code notification",
                    EmailTemplateID = "d-16815c0473d948acb2715a5001907e8c",
                    TemplateFieldsJson = "[\"auth_code\", \"user_name\"]",
                    IsActive = true
                },
                new NotifyTypes{
                    TypeID = 2,
                    Category = NotifyCategory.Purchase,
                    Description = "Purchase confirmation notification",
                    EmailTemplateID = "d-69ac862108b441d6b289875f5365c4d3",
                    TemplateFieldsJson = "[\"cart_items\", \"cart_price\", \"user_name\"]",
                    IsActive = true
                },
                new NotifyTypes{
                    TypeID = 3,
                    Category = NotifyCategory.PointsChange,
                    Description = "Notification for point changes",
                    EmailTemplateID = "d-0c016d5246e447d5873163bb0f9138b8",
                    TemplateFieldsJson = "[\"new_balance\", \"status_msg\", \"user_name\"]",
                    IsActive = true
                },
                new NotifyTypes{
                    TypeID = 4,
                    Category = NotifyCategory.SystemChange,
                    Description = "System or Sponsor account changes notification (includes account removal)",
                    EmailTemplateID = "d-8c3f3751f36a40a4820b5d14cd056386",
                    TemplateFieldsJson = "[\"message\", \"user_name\"]",
                    IsActive = true
                },
                new NotifyTypes{
                    TypeID = 5,
                    Category = NotifyCategory.AppStatus,
                    Description = "Driver application status notification",
                    EmailTemplateID = "d-b6f2e28c32e748ddafeab97761e74bb9",
                    TemplateFieldsJson = "[\"status\", \"status_message\", \"user_name\"]",
                    IsActive = true
                },
                new NotifyTypes{
                    TypeID = 6,
                    Category = NotifyCategory.OrderIssue,
                    Description = "Order update or issue notification",
                    EmailTemplateID = "d-f966dff0b760434f871016a1a9761600",
                    TemplateFieldsJson = "[\"order_id\", \"issue_details\", \"user_name\"]",
                    IsActive = true
                },
                new NotifyTypes{
                    TypeID = 7,
                    Category = NotifyCategory.PointsReport,
                    Description = "Summary points report",
                    EmailTemplateID = "d-972ccaa0eb9b427ea96436f8fd1af7c7",
                    TemplateFieldsJson = "[\"report_date\", \"total_points\", \"details\"]",
                    IsActive = true
                }
            );

            modelBuilder.Entity<Sponsors>().HasData(
                new Sponsors
                {
                    SponsorID = 1,
                    SponsorType = SponsorType.Trucking,
                    CompanyName = "TruckMasters Inc.",
                    PointDollarValue = 0.05m,
                    MilestoneThreshold = 0
                },
                new Sponsors
                {
                    SponsorID = 2,
                    SponsorType = SponsorType.Logistics,
                    CompanyName = "LogiPro Solutions",
                    PointDollarValue = 0.02m,
                    MilestoneThreshold = 0
                },
                new Sponsors
                {
                    SponsorID = 3,
                    SponsorType = SponsorType.Fleeting,
                    CompanyName = "FleetForce Partners",
                    PointDollarValue = 0.01m,
                    MilestoneThreshold = 0
                }
            );

            modelBuilder.Entity<FeedbackForms>().HasData(
                new FeedbackForms
                {
                    FeedbackID = 1,
                    FirstName = "Alice",
                    Email = "alice@example.com",
                    FeedbackCategory = FeedbackType.Suggestion,
                    Comments = "It would be great to have more achievments to earn.",
                    SubmissionDate = DateTime.UtcNow
                },
                new FeedbackForms
                {
                    FeedbackID = 2,
                    FirstName = "Bob",
                    Email = "bob@example.com",
                    FeedbackCategory = FeedbackType.BugReport,
                    Comments = "The dashboard takes too long to load.",
                    SubmissionDate = DateTime.UtcNow
                },
                new FeedbackForms
                {
                    FeedbackID = 3,
                    FirstName = "Charlie",
                    Email = "charlie@example.com",
                    FeedbackCategory = FeedbackType.Compliment,
                    Comments = "The new design is fantastic. Keep it up!",
                    SubmissionDate = DateTime.UtcNow
                },
                new FeedbackForms
                {
                    FeedbackID = 4,
                    FirstName = "Dana",
                    Email = "dana@example.com",
                    FeedbackCategory = FeedbackType.Complaint,
                    Comments = "The points system feels unfair. Please adjust.",
                    SubmissionDate = DateTime.UtcNow
                }
            );
        }
    }
}
