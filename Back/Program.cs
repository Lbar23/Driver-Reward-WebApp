using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Team16_WebApp_4910.Server.Models;
using Renci.SshNet;
using Team16_WebApp_4910.Server;

var builder = WebApplication.CreateBuilder(args);

// Any credentials needed (Secrets Manager)
var username = "gitgudadmin";
var password = "B%yuup#2559!";

// SSH and database connection setup
string sshHost = "3.136.81.78";
string sshUsername = "ubuntu";
string sshKeyPath = "C:\\Users\\ragas\\Documents\\Computer code\\4910\\cpsc-team16-key.pem"; //<- to secrets manager later

string dbHost = "team16-database.cpin0o6jvads.us-east-2.rds.amazonaws.com";
int dbPort = 3306;

using (var sshClient = new SshClient(sshHost, sshUsername, new PrivateKeyFile(sshKeyPath)))
{
    sshClient.Connect();
    var forwardedPort = new ForwardedPortLocal("127.0.0.1", (uint)dbPort, dbHost, (uint)dbPort);
    sshClient.AddForwardedPort(forwardedPort);
    forwardedPort.Start();

    var connection_string = builder.Configuration.GetConnectionString("DefaultConnection")
        .Replace("{Username}", username)
        .Replace("{Password}", password)
        .Replace(dbHost, "127.0.0.1");

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder =>
            {
                builder.WithOrigins("http://localhost:5173", "https://localhost:7284", "http://localhost:5062")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials(); // Important for cookie auth
            });
    });

    builder.Services.AddDbContext<AppDBContext>(options =>
        options.UseMySql(connection_string,
            new MySqlServerVersion(new Version(8, 0, 25)),
            options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

    // Add Identity services
    builder.Services.AddIdentity<Users, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders();

    // Configure cookie settings (if needed)
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/users/login";
        options.LogoutPath = "/users/logout";
        options.AccessDeniedPath = "/users/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
    });

    // Configure two-factor authentication
    builder.Services.Configure<IdentityOptions>(options =>
    {
        options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
    });

    builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
    builder.Services.AddTransient<IEmailService, EmailService>();
    
    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    app.UseRouting();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1"));
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAllOrigins");

    // Add authentication and authorization middleware
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.MapFallbackToFile("/index.html");

    app.Run();

    // Close and clean up the SSH connection
    forwardedPort.Stop();
    sshClient.Disconnect();
}