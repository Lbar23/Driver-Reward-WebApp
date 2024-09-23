using BCrypt.Net;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> RegisterUser(string username, string password, string email)
    {
        // Check if user already exists
        if (_context.Users.Any(u => u.Email == email))
        {
            throw new Exception("That email is already in use");
        }

        // Hash the password
        string passwordHash = BCrypt.HashPassword(password);

        // Create new user
        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            Role = UserRole.Driver,
            CreatedAt = DateTime.Now,
            LastLogin = null
        };

        // Save user to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }
}