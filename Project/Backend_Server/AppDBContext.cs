using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Backend_Server.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Backend_Server
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : IdentityDbContext<Users, IdentityRole<int>, int>(options)
    {
        public DbSet<About> About { get; set; }
        public DbSet<Sponsors> Sponsors { get; set; }
        public DbSet<Drivers> Drivers { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Purchases> Purchases { get; set; }
        public DbSet<PointTransactions> PointTransactions { get; set; }
        public DbSet<DriverApplications> DriverApplications { get; set; }
        public DbSet<AuditLog> AuditLog { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<Admins> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("AspNetUsers");
                entity.Property(e => e.UserType).IsRequired().HasConversion<string>();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("TIMESTAMP");
                entity.Property(e => e.NotifyPref).HasDefaultValue(NotificationPref.None);
            });

            modelBuilder.Entity<Sponsors>(entity =>
            {
                entity.ToTable("Sponsors");
                entity.HasKey(e => e.SponsorID);
                entity.HasOne<Users>().WithMany().HasForeignKey(d => d.UserID);
                entity.Property(e => e.SponsorType).IsRequired();
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PointDollarValue).HasPrecision(10, 2).HasDefaultValue(0.01m);
            });

            modelBuilder.Entity<About>(entity =>
            {
                entity.ToTable("About");
                entity.HasKey(e => e.Release);
                entity.Property(d => d.Team).HasDefaultValue(16);
                entity.Property(d => d.Version).IsRequired();
                entity.Property(d => d.Product).IsRequired();
                entity.Property(d => d.Description);
            });

            modelBuilder.Entity<Drivers>(entity =>
            {
                entity.ToTable("Drivers");
                entity.HasKey(e => e.UserID);
                entity.HasOne<Users>().WithMany().HasForeignKey(d => d.UserID);
                entity.HasOne<Sponsors>().WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.TotalPoints).HasDefaultValue(0);
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.ProductID);
                entity.HasOne<Sponsors>().WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PriceInPoints).IsRequired();
                entity.Property(e => e.Availability).HasDefaultValue(true);
                entity.Property(e => e.ExternalID).IsRequired();
            });

            modelBuilder.Entity<Purchases>(entity =>
            {
                entity.ToTable("Purchases");
                entity.HasKey(e => e.PurchaseID);
                entity.HasOne<Users>().WithMany().HasForeignKey(d => d.UserID);
                entity.HasOne<Purchases>().WithMany().HasForeignKey(d => d.PurchaseID);
                entity.Property(e => e.PointsSpent).IsRequired();
                entity.Property(e => e.PurchaseDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Status).HasConversion<string>()
                    .HasDefaultValue(OrderStatus.Ordered);
            });

            modelBuilder.Entity<PointTransactions>(entity =>
            {
                entity.ToTable("PointTransactions");
                entity.HasKey(e => e.TransactionID);
                entity.HasOne<Users>().WithMany().HasForeignKey(d => d.UserID);
                entity.HasOne<Sponsors>().WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.PointsChanged).IsRequired();
                entity.Property(e => e.TransactionDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Reason).IsRequired();
            });

            modelBuilder.Entity<DriverApplications>(entity =>
            {
                entity.ToTable("DriverApplications");
                entity.HasKey(e => e.ApplicationID);
                entity.HasOne<Users>().WithMany().HasForeignKey(d => d.UserID);
                entity.HasOne<Sponsors>().WithMany().HasForeignKey(d => d.SponsorID);
                entity.Property(e => e.Status).HasConversion<string>()
                    .HasDefaultValue(AppStatus.Submitted);
                entity.Property(e => e.ApplyDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.Reason).IsRequired().HasDefaultValue(string.Empty);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLog");
                entity.HasKey(e => e.LogID);
                entity.HasOne<Users>().WithMany().HasForeignKey(d => d.UserID);
                // entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Category).HasDefaultValue(AuditLogCategory.User).HasConversion<string>();
                entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Permissions>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(e => e.PermissionID);
                entity.Property(d => d.Role).IsRequired();
                entity.Property(d => d.Permission).HasConversion<string>();
            });

            modelBuilder.Entity<Admins>(entity =>
            {
                entity.ToTable("Admins");
                entity.HasKey(e => e.UserID);
                entity.HasOne<Users>().WithMany().HasForeignKey(d => d.UserID);
            });
        }
    }
}
