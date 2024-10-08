using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Team16_WebApp_4910.Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Team16_WebApp_4910.Server
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : IdentityDbContext<Users, IdentityRole<int>, int>(options)
    {
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

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.UserType).IsRequired().HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Sponsors>(entity =>
            {
                entity.ToTable("Sponsors");
                entity.HasKey(e => e.SponsorID);
                entity.HasOne(d => d.User).WithOne().HasForeignKey<Sponsors>(d => d.UserID);
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PointDollarValue).HasPrecision(10, 2).HasDefaultValue(0.01m);
            });

            modelBuilder.Entity<Drivers>(entity =>
            {
                entity.ToTable("Drivers");
                entity.HasKey(e => e.DriverID);
                entity.HasOne(d => d.User).WithOne().HasForeignKey<Drivers>(d => d.UserID);
                entity.HasOne(d => d.Sponsor).WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.TotalPoints).HasDefaultValue(0);
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.ProductID);
                entity.HasOne(d => d.Sponsor).WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PriceInPoints).IsRequired();
                entity.Property(e => e.Availability).HasDefaultValue(true);
            });

            modelBuilder.Entity<Purchases>(entity =>
            {
                entity.ToTable("Purchases");
                entity.HasKey(e => e.PurchaseID);
                entity.HasOne(d => d.Driver).WithMany().HasForeignKey(d => d.DriverID);
                entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductID);
                entity.Property(e => e.PointsSpent).IsRequired();
                entity.Property(e => e.PurchaseDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Status).HasDefaultValue("Pending").HasConversion<string>();
            });

            modelBuilder.Entity<PointTransactions>(entity =>
            {
                entity.ToTable("PointTransactions");
                entity.HasKey(e => e.TransactionID);
                entity.HasOne(d => d.Driver).WithMany().HasForeignKey(d => d.DriverID);
                entity.HasOne(d => d.Sponsor).WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.PointsChanged).IsRequired();
                entity.Property(e => e.TransactionDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<DriverApplications>(entity =>
            {
                entity.ToTable("DriverApplications");
                entity.HasKey(e => e.ApplicationID);
                entity.HasOne(d => d.Driver).WithMany().HasForeignKey(d => d.DriverID);
                entity.HasOne(d => d.Sponsor).WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.Status).HasDefaultValue("Pending").HasConversion<string>();
                entity.Property(e => e.ApplyDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLog");
                entity.HasKey(e => e.LogID);
                entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserID);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).HasDefaultValue(AuditLogCategory.User).HasConversion<string>();
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}