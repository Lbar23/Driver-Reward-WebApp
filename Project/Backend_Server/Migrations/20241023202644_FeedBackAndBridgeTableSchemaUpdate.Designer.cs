﻿// <auto-generated />
using System;
using Backend_Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Backend_Server.Migrations
{
    [DbContext(typeof(AppDBContext))]
    [Migration("20241023202644_FeedBackAndBridgeTableSchemaUpdate")]
    partial class FeedBackAndBridgeTableSchemaUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Backend_Server.Models.About", b =>
                {
                    b.Property<DateOnly>("Release")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("DATE")
                        .HasDefaultValueSql("(CURRENT_DATE)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Product")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Team")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(16);

                    b.Property<int>("Version")
                        .HasColumnType("int");

                    b.HasKey("Release");

                    b.ToTable("About", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Admins", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("UserID");

                    b.ToTable("Admins", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.AuditLog", b =>
                {
                    b.Property<int>("LogID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("LogID"));

                    b.Property<string>("Category")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("longtext")
                        .HasDefaultValue("User");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Timestamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("LogID");

                    b.HasIndex("UserID");

                    b.ToTable("AuditLog", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.DriverApplications", b =>
                {
                    b.Property<int>("ApplicationID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("ApplicationID"));

                    b.Property<DateOnly>("ApplyDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("DATE")
                        .HasDefaultValueSql("(CURRENT_DATE)");

                    b.Property<DateOnly?>("ProcessedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("DATE")
                        .HasDefaultValueSql("(CURRENT_DATE)");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("longtext")
                        .HasDefaultValue("");

                    b.Property<int>("SponsorID")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("longtext")
                        .HasDefaultValue("Submitted");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("ApplicationID");

                    b.HasIndex("SponsorID");

                    b.HasIndex("UserID");

                    b.ToTable("DriverApplications", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Drivers", b =>
                {
                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.Property<int>("SponsorID")
                        .HasColumnType("int");

                    b.Property<int>("TotalPoints")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("UserID");

                    b.HasIndex("SponsorID");

                    b.ToTable("Drivers", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.FeedbackForms", b =>
                {
                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("varchar(2000)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<DateTime>("SubmissionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("Name");

                    b.ToTable("FeedbackForms", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Permissions", b =>
                {
                    b.Property<int>("PermissionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("PermissionID"));

                    b.Property<string>("Permission")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("PermissionID");

                    b.ToTable("Permissions", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.PointTransactions", b =>
                {
                    b.Property<int>("TransactionID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("TransactionID"));

                    b.Property<int>("PointsChanged")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("SponsorID")
                        .HasColumnType("int");

                    b.Property<DateTime>("TransactionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("TransactionID");

                    b.HasIndex("SponsorID");

                    b.HasIndex("UserID");

                    b.ToTable("PointTransactions", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Products", b =>
                {
                    b.Property<int>("ProductID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("ProductID"));

                    b.Property<bool>("Availability")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasDefaultValue(true);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ExternalID")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("PriceInPoints")
                        .HasColumnType("int");

                    b.Property<int>("SponsorID")
                        .HasColumnType("int");

                    b.HasKey("ProductID");

                    b.HasIndex("SponsorID");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Purchases", b =>
                {
                    b.Property<int>("PurchaseID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("PurchaseID"));

                    b.Property<int>("DriverUserID")
                        .HasColumnType("int");

                    b.Property<int>("PointsSpent")
                        .HasColumnType("int");

                    b.Property<int>("ProductID")
                        .HasColumnType("int");

                    b.Property<DateTime>("PurchaseDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Status")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("longtext")
                        .HasDefaultValue("Ordered");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("PurchaseID");

                    b.HasIndex("DriverUserID");

                    b.HasIndex("ProductID");

                    b.HasIndex("UserID");

                    b.ToTable("Purchases", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.SponsorDrivers", b =>
                {
                    b.Property<int>("SponsorID")
                        .HasColumnType("int");

                    b.Property<int>("DriverID")
                        .HasColumnType("int");

                    b.HasKey("SponsorID", "DriverID");

                    b.HasIndex("DriverID");

                    b.ToTable("SponsorDrivers", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Sponsors", b =>
                {
                    b.Property<int>("SponsorID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("SponsorID"));

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<decimal>("PointDollarValue")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(10, 2)
                        .HasColumnType("decimal(10,2)")
                        .HasDefaultValue(0.01m);

                    b.Property<string>("SponsorType")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserID")
                        .HasColumnType("int");

                    b.HasKey("SponsorID");

                    b.HasIndex("UserID");

                    b.ToTable("Sponsors", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Users", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TIMESTAMP")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime(6)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<int>("NotifyPref")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("UserType")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Backend_Server.Models.Admins", b =>
                {
                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend_Server.Models.AuditLog", b =>
                {
                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend_Server.Models.DriverApplications", b =>
                {
                    b.HasOne("Backend_Server.Models.Sponsors", null)
                        .WithMany()
                        .HasForeignKey("SponsorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend_Server.Models.Drivers", b =>
                {
                    b.HasOne("Backend_Server.Models.Sponsors", null)
                        .WithMany()
                        .HasForeignKey("SponsorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend_Server.Models.PointTransactions", b =>
                {
                    b.HasOne("Backend_Server.Models.Sponsors", null)
                        .WithMany()
                        .HasForeignKey("SponsorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend_Server.Models.Products", b =>
                {
                    b.HasOne("Backend_Server.Models.Sponsors", null)
                        .WithMany()
                        .HasForeignKey("SponsorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend_Server.Models.Purchases", b =>
                {
                    b.HasOne("Backend_Server.Models.Drivers", "Driver")
                        .WithMany()
                        .HasForeignKey("DriverUserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend_Server.Models.Products", "Product")
                        .WithMany()
                        .HasForeignKey("ProductID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Driver");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Backend_Server.Models.SponsorDrivers", b =>
                {
                    b.HasOne("Backend_Server.Models.Drivers", "Driver")
                        .WithMany("SponsorDriver")
                        .HasForeignKey("DriverID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend_Server.Models.Sponsors", "Sponsor")
                        .WithMany("SponsorDriver")
                        .HasForeignKey("SponsorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Driver");

                    b.Navigation("Sponsor");
                });

            modelBuilder.Entity("Backend_Server.Models.Sponsors", b =>
                {
                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("Backend_Server.Models.Users", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Backend_Server.Models.Drivers", b =>
                {
                    b.Navigation("SponsorDriver");
                });

            modelBuilder.Entity("Backend_Server.Models.Sponsors", b =>
                {
                    b.Navigation("SponsorDriver");
                });
#pragma warning restore 612, 618
        }
    }
}
