using Microsoft.EntityFrameworkCore;
using System;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Define DbSet properties for your entities here
    // public DbSet<YourEntity> YourEntities { get; set; }
    public DbSet<User> Users { get; set; }

}