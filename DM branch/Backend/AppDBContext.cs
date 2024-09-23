//Sprint 2 Implementation


using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Sponsors> Sponsors { get; set; }
        public DbSet<Drivers> Drivers { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Purchases> Purchases { get; set; }
        public DbSet<PointTransactions> PointTransactions { get; set; }
        public DbSet<DriverApplications> DriverApplications { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>(static entity =>
            {
                entity.HasKey(static e => e.UserID);
                entity.Property(static e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(static e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(static e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(static e => e.Role).IsRequired().HasConversion<string>();
                entity.Property(static e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Sponsors>(static entity =>
            {
                entity.HasKey(static e => e.SponsorID);
                entity.HasOne(static d => d.User).WithOne().HasForeignKey<Sponsors>(static d => d.UserID);
                entity.Property(static e => e.CompanyName).IsRequired().HasMaxLength(100);
                entity.Property(static e => e.PointDollarValue).HasPrecision(10, 2).HasDefaultValue(0.01m);
            });

            modelBuilder.Entity<Drivers>(static entity =>
            {
                entity.HasKey(static e => e.DriverID);
                entity.HasOne(static d => d.User).WithOne().HasForeignKey<Drivers>(static d => d.UserID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.TotalPoints).HasDefaultValue(0);
            });

            modelBuilder.Entity<Products>(static entity =>
            {
                entity.HasKey(static e => e.ProductID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(static e => e.PriceInPoints).IsRequired();
                entity.Property(static e => e.Availability).HasDefaultValue(true);
            });

            modelBuilder.Entity<Purchases>(static entity =>
            {
                entity.HasKey(static e => e.PurchaseID);
                entity.HasOne(static d => d.Driver).WithMany().HasForeignKey(static d => d.DriverID);
                entity.HasOne(static d => d.Product).WithMany().HasForeignKey(static d => d.ProductID);
                entity.Property(static e => e.PointsSpent).IsRequired();
                entity.Property(static e => e.PurchaseDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(static e => e.Status).HasDefaultValue("Pending").HasConversion<string>();
            });

            modelBuilder.Entity<PointTransactions>(static entity =>
            {
                entity.HasKey(static e => e.TransactionID);
                entity.HasOne(static d => d.Driver).WithMany().HasForeignKey(static d => d.DriverID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.PointsChanged).IsRequired();
                entity.Property(static e => e.TransactionDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<DriverApplications>(static entity =>
            {
                entity.HasKey(static e => e.ApplicationID);
                entity.HasOne(static d => d.Driver).WithMany().HasForeignKey(static d => d.DriverID);
                entity.HasOne(static d => d.Sponsor).WithMany().HasForeignKey(static d => d.SponsorID);
                entity.Property(static e => e.Status).HasDefaultValue("Pending").HasConversion<string>();
                entity.Property(static e => e.ApplyDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<AuditLog>(static entity =>
            {
                entity.HasKey(static e => e.LogID);
                entity.HasOne(static d => d.User).WithMany().HasForeignKey(static d => d.UserID);
                entity.Property(static e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(static e => e.Category).HasDefaultValue(AuditLogCategory.User).HasConversion<string>();
                entity.Property(static e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}