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
        //As a note; change these to required once everything is finalized. It WILL break if changed now...
        //Keep this as a note later; if nothing happens if someone else makes them required, then continue
        //Otherwise, wait till Saturday/Sunday and delete these comments
        public DbSet<About> About { get; set; }
        public DbSet<Sponsors> Sponsors { get; set; }
        public DbSet<Drivers> Drivers { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Purchases> Purchases { get; set; }
        public DbSet<PointTransactions> PointTransactions { get; set; }
        public DbSet<DriverApplications> DriverApplications { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<Admins> Admins { get; set; }
        public DbSet<SponsorDrivers> SponsorDrivers { get; set; }
        public DbSet<FeedbackForms> FeedbackForms { get; set; }
        public DbSet<SponsorUsers> SponsorUsers { get; set;} 
        // for the procedure calls
        public DbSet<SalesSummary> SalesSummaries { get; set; }
        public DbSet<SalesDetail> SalesDetails { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<DriverPoints> DriverPoints { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

             // breaks if not here rn
            modelBuilder.Entity<SalesSummary>().HasNoKey();
            modelBuilder.Entity<SalesDetail>().HasNoKey();
            modelBuilder.Entity<InvoiceDetail>().HasNoKey();
            modelBuilder.Entity<DriverPoints>().HasNoKey();
            modelBuilder.Entity<AuditLog>().HasNoKey();

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("AspNetUsers");

                entity.Property(e => e.UserType)
                    .IsRequired()
                    .HasConversion<string>();

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");
                    
                entity.Property(e => e.NotifyPref)
                    .HasDefaultValue(NotificationPref.None);
            });

            modelBuilder.Entity<Sponsors>(entity =>
            {
                entity.ToTable("Sponsors");

                entity.HasKey(e => e.SponsorID);

                entity.HasOne<Users>()
                    .WithMany()
                    .HasForeignKey(d => d.UserID);

                entity.Property(e => e.SponsorType)
                    .IsRequired();

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PointDollarValue)
                    .HasPrecision(10, 2)
                    .HasDefaultValue(0.01m);
            });

            modelBuilder.Entity<About>(entity =>
            {
                entity.ToTable("About");

                entity.HasKey(e => e.Release);

                entity.Property(e => e.Release)
                    .HasDefaultValueSql("(CURRENT_DATE)")
                    .HasColumnType("DATE");

                entity.Property(d => d.Team)
                    .HasDefaultValue(16);

                entity.Property(d => d.Version)
                    .IsRequired();

                entity.Property(d => d.Product)
                    .IsRequired();

                entity.Property(d => d.Description)
                    .IsRequired();
            });

            modelBuilder.Entity<Drivers>(entity =>
            {
                entity.ToTable("Drivers");

                entity.HasKey(e => e.UserID);

                entity.HasOne<Users>()
                    .WithMany()
                    .HasForeignKey(d => d.UserID);
                
                entity.Property(e => e.FirstName)
                    //.IsRequired() uncomment once fully injected
                    .HasMaxLength(50);
                
                entity.Property(e => e.LastName)
                    //.IsRequired() same here...
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(e => e.ProductID);

                entity.HasOne<Sponsors>()
                    .WithMany()
                    .HasForeignKey(d => d.SponsorID);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.PriceInPoints)
                    .IsRequired();

                entity.Property(e => e.Availability)
                    .HasDefaultValue(true);

                entity.Property(e => e.ExternalID)
                    .IsRequired();
            });

            modelBuilder.Entity<Purchases>(entity =>
            {
                entity.ToTable("Purchases");

                entity.HasKey(e => e.PurchaseID);

                entity.HasOne<Users>()
                    .WithMany()
                    .HasForeignKey(d => d.SponsorID);

                entity.HasOne(p => p.Driver)
                    .WithMany()
                    .HasForeignKey("DriverID");  // Explicitly use DriverID instead of DriverUserID the context is seeing still

                entity.HasOne(p => p.Product)
                    .WithMany()
                    .HasForeignKey("ProductID");

                entity.Property(e => e.PointsSpent)
                    .IsRequired();

                entity.Property(e => e.PurchaseDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasDefaultValue(OrderStatus.Ordered);
            });

            modelBuilder.Entity<PointTransactions>(entity =>
            {
                entity.ToTable("PointTransactions");

                entity.HasKey(e => e.TransactionID);

                entity.HasOne<Users>()
                    .WithMany()
                    .HasForeignKey(d => d.UserID);
                
                entity.HasOne<Sponsors>()
                    .WithMany()
                    .HasForeignKey(d => d.SponsorID);

                entity.Property(e => e.PointsChanged)
                    .IsRequired();

                entity.Property(e => e.TransactionDate)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");

                entity.Property(e => e.Reason)
                    .IsRequired();
            });

            modelBuilder.Entity<DriverApplications>(entity =>
            {
                entity.ToTable("DriverApplications");

                entity.HasKey(e => e.ApplicationID);

                entity.HasOne<Users>()
                    .WithMany()
                    .HasForeignKey(d => d.UserID);

                entity.HasOne<Sponsors>()
                    .WithMany()
                    .HasForeignKey(d => d.SponsorID);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasDefaultValue(AppStatus.Submitted);

                entity.Property(e => e.ApplyDate)
                    .HasColumnType("DATE")
                    .HasDefaultValueSql("(CURRENT_DATE)");

                entity.Property(e => e.ProcessedDate)
                    .HasDefaultValueSql("(CURRENT_DATE)")
                    .HasColumnType("DATE");

                entity.Property(e => e.Reason)
                    .IsRequired()
                    .HasDefaultValue(string.Empty);
            });

            modelBuilder.Entity<AuditLogs>(entity =>
            {
                entity.ToTable("AuditLogs");

                entity.HasKey(e => e.LogID);

                entity.HasOne<Users>()
                    .WithMany()
                    .HasForeignKey(d => d.UserID);

                // entity.Property(e => e.Action)
                    // .IsRequired()
                    // .HasMaxLength(50);

                entity.Property(e => e.Category)
                    .HasDefaultValue(AuditLogCategory.User)
                    .HasConversion<string>();
                
                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("TIMESTAMP");
            });

            modelBuilder.Entity<Admins>(entity =>
            {
                entity.ToTable("Admins");

                entity.HasKey(e => e.UserID);

                entity.HasOne<Users>()
                    .WithMany()
                    .HasForeignKey(d => d.UserID);
            });

            modelBuilder.Entity<SponsorDrivers>(entity =>
            {
                entity.ToTable("SponsorDrivers");

                entity.HasKey(sd => new { sd.SponsorID, sd.DriverID });

                entity.Property(sd => sd.Points)
                    .HasDefaultValue(0)
                    .IsRequired();

                entity.HasOne(sd => sd.Driver)
                    .WithMany(d => d.SponsorDrivers)
                    .HasForeignKey(sd => sd.DriverID);
                    
                entity.HasOne(sd => sd.Sponsor)
                    .WithMany(d => d.SponsorDrivers)
                    .HasForeignKey(sd => sd.SponsorID);
            });

            modelBuilder.Entity<FeedbackForms>(entity =>
            {
               entity.ToTable("FeedbackForms"); 

               entity.HasKey(f => f.Name);

               entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(256);
                    
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(2000);
                    
                entity.Property(e => e.SubmissionDate)
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<SponsorUsers>(entity => 
            {
                entity.ToTable("SponsorUsers");

                entity.HasKey(su => new { su.SponsorID, su.UserID });

                entity.Property(su => su.IsPrimarySponsor)
                    .HasDefaultValue(false);

                entity.Property(su => su.JoinDate)
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(su => su.SponsorRole)
                    .HasConversion<string>()
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue(SponsorRole.Standard);

                entity.HasOne(su => su.User)
                    .WithMany(u => u.SponsorUsers)
                    .HasForeignKey(su => su.UserID);
                
                entity.HasOne(su => su.Sponsor)
                    .WithMany(s => s.SponsorUsers)
                    .HasForeignKey(su => su.SponsorID);

                entity.HasIndex(su => new { su.SponsorID, su.IsPrimarySponsor});
            });
        }
    }
}
