using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Team16_WebApp_4910.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Team16_WebApp_4910.Server
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
    {
        //override
        public DbSet<Users> Users { get; set; }
        public DbSet<Admins> Admins { get; set; }
        public DbSet<Sponsors> Sponsors { get; set; }
        public DbSet<Drivers> Drivers { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Purchases> Purchases { get; set; }
        public DbSet<PointTransactions> PointTransactions { get; set; }
        public DbSet<DriverApplications> DriverApplications { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>(static entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(static e => e.UserID);
                entity.Property(static e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(static e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(static e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(static e => e.UserType).IsRequired().HasConversion<string>();
                entity.Property(static e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Admins>(static entity =>
            {
                entity.ToTable("Admins");
                entity.HasKey(static e => e.AdminID);
                entity.HasOne(static d => d.User).WithOne().HasForeignKey<Admins>(static d => d.UserID);
            });

            modelBuilder.Entity<Sponsors>(static entity =>
            {
                entity.ToTable("Sponsors");
                entity.HasKey(static e => e.SponsorID);
                entity.HasOne(static d => d.User).WithOne().HasForeignKey<Sponsors>(static d => d.UserID);
                entity.Property(static e => e.CompanyName).IsRequired().HasMaxLength(100);
                entity.Property(static e => e.PointDollarValue).HasPrecision(10, 2).HasDefaultValue(0.01m);
            });

            modelBuilder.Entity<Drivers>(static entity =>
            {
                entity.ToTable("Drivers");
                entity.HasKey(static e => e.DriverID);
                entity.HasOne(static d => d.User).WithOne().HasForeignKey<Drivers>(static d => d.UserID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.TotalPoints).HasDefaultValue(0);
            });

            modelBuilder.Entity<Products>(static entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(static e => e.ProductID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(static e => e.PriceInPoints).IsRequired();
                entity.Property(static e => e.Availability).HasDefaultValue(true);
            });

            modelBuilder.Entity<Purchases>(static entity =>
            {
                entity.ToTable("Purchases");
                entity.HasKey(static e => e.PurchaseID);
                entity.HasOne(static d => d.Driver).WithMany().HasForeignKey(static d => d.DriverID);
                entity.HasOne(static d => d.Product).WithMany().HasForeignKey(static d => d.ProductID);
                entity.Property(static e => e.PointsSpent).IsRequired();
                entity.Property(static e => e.PurchaseDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(static e => e.Status).HasDefaultValue("Pending").HasConversion<string>();
            });

            modelBuilder.Entity<PointTransactions>(static entity =>
            {
                entity.ToTable("PointTransactions");
                entity.HasKey(static e => e.TransactionID);
                entity.HasOne(static d => d.Driver).WithMany().HasForeignKey(static d => d.DriverID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.PointsChanged).IsRequired();
                entity.Property(static e => e.TransactionDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<DriverApplications>(static entity =>
            {
                entity.ToTable("DriverApplications");
                entity.HasKey(static e => e.ApplicationID);
                entity.HasOne(static d => d.Driver).WithMany().HasForeignKey(static d => d.DriverID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.Status).HasDefaultValue("Pending").HasConversion<string>();
                entity.Property(static e => e.ApplyDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<AuditLog>(static entity =>
            {
                entity.ToTable("AuditLog");
                entity.HasKey(static e => e.LogID);
                entity.HasOne(static d => d.User).WithMany().HasForeignKey(static d => d.UserID);
                entity.Property(static e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(static e => e.Category).HasDefaultValue(AuditLogCategory.User).HasConversion<string>();
                entity.Property(static e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(entity => 
            {
                entity.HasKey(e => new {e.LoginProvider, e.ProviderKey});
            });
        }
    }
}