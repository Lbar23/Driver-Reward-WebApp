using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend_Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "About",
                columns: table => new
                {
                    Release = table.Column<DateOnly>(type: "DATE", nullable: false, defaultValueSql: "(CURRENT_DATE)", comment: "Release date of the current version of the program."),
                    Team = table.Column<int>(type: "int", nullable: false, defaultValue: 16, comment: "Team number responsible for the project. Immutable."),
                    Version = table.Column<int>(type: "int", nullable: false, comment: "Current version of the program."),
                    Product = table.Column<string>(type: "longtext", nullable: false, comment: "Name of the program.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false, comment: "Description of the program.")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_About", x => x.Release);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FeedbackForms",
                columns: table => new
                {
                    FeedbackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeedbackCategory = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comments = table.Column<string>(type: "VARCHAR(1000)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubmissionDate = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackForms", x => x.FeedbackID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotifyTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "Category of notification, such as TwoFA, Purchase, PointsChange, etc.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, comment: "Optional description of the notification type.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailTemplateID = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, comment: "SendGrid template ID for email notifications.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TemplateFieldsJson = table.Column<string>(type: "VARCHAR(1000)", nullable: true, defaultValue: "[]", comment: "JSON array defining expected dynamic fields for templates.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true, comment: "Indicates if the notification type is currently active.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifyTypes", x => x.TypeID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    SponsorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SponsorType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, comment: "Defines the type of sponsor (Logistics, Trucking, etc.)")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "Name of the sponsor company")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointDollarValue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0.01m, comment: "Dollar value of one point, default is 0.01 USD"),
                    MilestoneThreshold = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "If milestones aren't enabled the value is 0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.SponsorID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    RoleID = table.Column<int>(type: "int", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<string>(type: "char(2)", fixedLength: true, maxLength: 2, nullable: false, comment: "Two-character state abbreviation (ex. SC for South Carolina)")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProfileImgUrl = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NotifyPref = table.Column<string>(type: "longtext", nullable: false, defaultValue: "None")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_AspNetRoles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SponsorID = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false, comment: "The name of the product.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, comment: "The category the product belongs to.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false, defaultValue: "", comment: "Parsed description of the product.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CurrencyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, comment: "The monetary price of the product."),
                    PriceInPoints = table.Column<int>(type: "int", nullable: false, comment: "The equivalent price of the product in points."),
                    ExternalID = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, comment: "External ID used to track the product from the external source, eBay API.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, comment: "The URL for the product's primary image.")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Availability = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true, comment: "Indicates whether the product is currently available for purchase.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductID);
                    table.ForeignKey(
                        name: "FK_Products_Sponsors",
                        column: x => x.SponsorID,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionSuccess = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    AdditionalDetails = table.Column<string>(type: "VARCHAR(1000)", nullable: true, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.LogID);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NotificationHistory",
                columns: table => new
                {
                    InstanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    NotifyTypeID = table.Column<int>(type: "int", nullable: true),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false, comment: "Indicates whether the notification was successfully sent (true) or failed (false)."),
                    NotifyDate = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Timestamp of when the notification was attempted.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationHistory", x => x.InstanceID);
                    table.ForeignKey(
                        name: "FK_NotificationHistory_NotifyTypes",
                        column: x => x.NotifyTypeID,
                        principalTable: "NotifyTypes",
                        principalColumn: "TypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationHistory_Users",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SponsorDrivers",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SponsorID = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false, defaultValue: 0, comment: "Points specific to this sponsor"),
                    MilestoneLevel = table.Column<int>(type: "int", nullable: false, comment: "Optional milestone level for tracking performance"),
                    DriverPointValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false, defaultValue: 1m, comment: "Custom point value for driver under this sponsor")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorDrivers", x => new { x.UserID, x.SponsorID });
                    table.ForeignKey(
                        name: "FK_SponsorDrivers_Driver",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SponsorDrivers_Sponsor",
                        column: x => x.SponsorID,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SponsorUsers",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false),
                    SponsorID = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false, comment: "Indicates if this user is the primary sponsor representative."),
                    JoinDate = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Date the user joined the sponsor organization.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SponsorUsers", x => new { x.UserID, x.SponsorID });
                    table.ForeignKey(
                        name: "FK_SponsorUsers_Sponsor",
                        column: x => x.SponsorID,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SponsorUsers_User",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DriverApplications",
                columns: table => new
                {
                    ApplicationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SponsorID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplyDate = table.Column<DateOnly>(type: "DATE", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcessedDate = table.Column<DateOnly>(type: "DATE", nullable: true),
                    ProcessReason = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comments = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverApplications", x => x.ApplicationID);
                    table.ForeignKey(
                        name: "FK_DriverApplications_SponsorDrivers_SponsorID_UserID",
                        columns: x => new { x.SponsorID, x.UserID },
                        principalTable: "SponsorDrivers",
                        principalColumns: new[] { "UserID", "SponsorID" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PointTransactions",
                columns: table => new
                {
                    TransactionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SponsorID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    PointsChanged = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActionType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionDate = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PointTransactions", x => x.TransactionID);
                    table.ForeignKey(
                        name: "FK_PointTransactions_SponsorDrivers_SponsorID_UserID",
                        columns: x => new { x.SponsorID, x.UserID },
                        principalTable: "SponsorDrivers",
                        principalColumns: new[] { "UserID", "SponsorID" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    PurchaseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SponsorID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    TotalPointsSpent = table.Column<int>(type: "int", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "TIMESTAMP", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.PurchaseID);
                    table.ForeignKey(
                        name: "FK_Purchases_SponsorDrivers_SponsorID_UserID",
                        columns: x => new { x.SponsorID, x.UserID },
                        principalTable: "SponsorDrivers",
                        principalColumns: new[] { "UserID", "SponsorID" },
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PurchaseProducts",
                columns: table => new
                {
                    PurchaseID = table.Column<int>(type: "int", nullable: false),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    PurchasedProductName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PurchasedUnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    PointsSpent = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseProducts", x => new { x.PurchaseID, x.ProductID });
                    table.ForeignKey(
                        name: "FK_PurchaseProducts_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseProducts_Purchases_PurchaseID",
                        column: x => x.PurchaseID,
                        principalTable: "Purchases",
                        principalColumn: "PurchaseID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "About",
                columns: new[] { "Release", "Description", "Product", "Team", "Version" },
                values: new object[,]
                {
                    { new DateOnly(2024, 9, 9), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 1 },
                    { new DateOnly(2024, 9, 16), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 2 },
                    { new DateOnly(2024, 9, 23), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 3 },
                    { new DateOnly(2024, 9, 30), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 4 },
                    { new DateOnly(2024, 10, 7), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 5 },
                    { new DateOnly(2024, 10, 14), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 6 },
                    { new DateOnly(2024, 10, 21), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 7 },
                    { new DateOnly(2024, 10, 28), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 8 },
                    { new DateOnly(2024, 11, 4), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 9 },
                    { new DateOnly(2024, 11, 11), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 10 },
                    { new DateOnly(2024, 11, 18), "Our Program, GitGud Drivers, empowers companies in the trucking industry to reward positive driver behavior with redeemable points.", "GitGud Drivers", 16, 11 }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, null, "Admin", "ADMIN" },
                    { 2, null, "Sponsor", "SPONSOR" },
                    { 3, null, "Driver", "DRIVER" },
                    { 4, null, "Guest", "GUEST" }
                });

            migrationBuilder.InsertData(
                table: "FeedbackForms",
                columns: new[] { "FeedbackID", "Comments", "Email", "FeedbackCategory", "FirstName", "SubmissionDate" },
                values: new object[,]
                {
                    { 1, "It would be great to have more achievments to earn.", "alice@example.com", "Suggestion", "Alice", new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7186) },
                    { 2, "The dashboard takes too long to load.", "bob@example.com", "BugReport", "Bob", new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7188) },
                    { 3, "The new design is fantastic. Keep it up!", "charlie@example.com", "Compliment", "Charlie", new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7191) },
                    { 4, "The points system feels unfair. Please adjust.", "dana@example.com", "Complaint", "Dana", new DateTime(2024, 12, 2, 1, 55, 4, 262, DateTimeKind.Utc).AddTicks(7193) }
                });

            migrationBuilder.InsertData(
                table: "NotifyTypes",
                columns: new[] { "TypeID", "Category", "Description", "EmailTemplateID", "IsActive", "TemplateFieldsJson" },
                values: new object[,]
                {
                    { 1, "Auth", "Authentication code notification", "d-16815c0473d948acb2715a5001907e8c", true, "[\"auth_code\", \"user_name\"]" },
                    { 2, "Purchase", "Purchase confirmation notification", "d-69ac862108b441d6b289875f5365c4d3", true, "[\"cart_items\", \"cart_price\", \"user_name\"]" },
                    { 3, "PointsChange", "Notification for point changes", "d-0c016d5246e447d5873163bb0f9138b8", true, "[\"new_balance\", \"status_msg\", \"user_name\"]" },
                    { 4, "SystemChange", "System or Sponsor account changes notification (includes account removal)", "d-8c3f3751f36a40a4820b5d14cd056386", true, "[\"message\", \"user_name\"]" },
                    { 5, "AppStatus", "Driver application status notification", "d-b6f2e28c32e748ddafeab97761e74bb9", true, "[\"status\", \"status_message\", \"user_name\"]" },
                    { 6, "OrderIssue", "Order update or issue notification", "d-f966dff0b760434f871016a1a9761600", true, "[\"order_id\", \"issue_details\", \"user_name\"]" },
                    { 7, "PointsReport", "Summary points report", "d-972ccaa0eb9b427ea96436f8fd1af7c7", true, "[\"report_date\", \"total_points\", \"details\"]" }
                });

            migrationBuilder.InsertData(
                table: "Sponsors",
                columns: new[] { "SponsorID", "CompanyName", "PointDollarValue", "SponsorType" },
                values: new object[,]
                {
                    { 1, "TruckMasters Inc.", 0.05m, "Trucking" },
                    { 2, "LogiPro Solutions", 0.02m, "Logistics" },
                    { 3, "FleetForce Partners", 0.01m, "Fleeting" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "Permission", "ManageUsers", 1 },
                    { 2, "Permission", "CanImpersonate", 1 },
                    { 3, "Permission", "ViewReports", 1 },
                    { 4, "Permission", "ViewAuditLogs", 1 },
                    { 5, "Permission", "ManageCatalog", 2 },
                    { 6, "Permission", "ManageApplications", 2 },
                    { 7, "Permission", "ManageDrivers", 2 },
                    { 8, "Permission", "CanImpersonate", 2 },
                    { 9, "Permission", "ViewReports", 2 },
                    { 10, "Permission", "ViewAuditLogs", 2 },
                    { 11, "Permission", "RedeemPoints", 2 },
                    { 12, "Permission", "RedeemPoints", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_RoleID",
                table: "AspNetUsers",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Category",
                table: "AuditLogs",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserID",
                table: "AuditLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_DriverApplications_ApplyDate",
                table: "DriverApplications",
                column: "ApplyDate");

            migrationBuilder.CreateIndex(
                name: "IX_DriverApplications_SponsorID_UserID",
                table: "DriverApplications",
                columns: new[] { "SponsorID", "UserID" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverApplications_Status",
                table: "DriverApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackForms_Email",
                table: "FeedbackForms",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackForms_FeedbackCategory",
                table: "FeedbackForms",
                column: "FeedbackCategory");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackForms_SubmissionDate",
                table: "FeedbackForms",
                column: "SubmissionDate");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistory_NotifyDate",
                table: "NotificationHistory",
                column: "NotifyDate");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistory_NotifyTypeID",
                table: "NotificationHistory",
                column: "NotifyTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationHistory_UserID",
                table: "NotificationHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_NotifyTypes_Category",
                table: "NotifyTypes",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_SponsorDriver",
                table: "PointTransactions",
                columns: new[] { "SponsorID", "UserID" });

            migrationBuilder.CreateIndex(
                name: "IX_PointTransactions_TransactionDate",
                table: "PointTransactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SponsorID",
                table: "Products",
                column: "SponsorID");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseProducts_ProductID",
                table: "PurchaseProducts",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_PurchaseDate",
                table: "Purchases",
                column: "PurchaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_SponsorID_UserID",
                table: "Purchases",
                columns: new[] { "SponsorID", "UserID" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDrivers_DriverSponsor",
                table: "SponsorDrivers",
                columns: new[] { "UserID", "SponsorID" });

            migrationBuilder.CreateIndex(
                name: "IX_SponsorDrivers_SponsorID",
                table: "SponsorDrivers",
                column: "SponsorID");

            migrationBuilder.CreateIndex(
                name: "IX_SponsorUsers_SponsorPrimary",
                table: "SponsorUsers",
                columns: new[] { "SponsorID", "IsPrimary" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "About");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DriverApplications");

            migrationBuilder.DropTable(
                name: "FeedbackForms");

            migrationBuilder.DropTable(
                name: "NotificationHistory");

            migrationBuilder.DropTable(
                name: "PointTransactions");

            migrationBuilder.DropTable(
                name: "PurchaseProducts");

            migrationBuilder.DropTable(
                name: "SponsorUsers");

            migrationBuilder.DropTable(
                name: "NotifyTypes");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "SponsorDrivers");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Sponsors");

            migrationBuilder.DropTable(
                name: "AspNetRoles");
        }
    }
}
