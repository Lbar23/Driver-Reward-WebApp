using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
}

public enum UserRole
{
    Driver,
    Sponsor,
    Admin
}

public class User
{
    public int UserID { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; }
    public DateTime LastLogin { get; set; }

}