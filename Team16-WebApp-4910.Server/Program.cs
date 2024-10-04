using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.SSHCertificates;
using Microsoft.OpenApi.Models;
using Renci.SshNet.Common;
using Team16_WebApp_4910.Server;
using Team16_WebApp_4910.Server.Models;
using Renci.SshNet;

var builder = WebApplication.CreateBuilder(args);

// Any credentials needed (Secrets Manager)
var username = builder.Configuration["DBCredentials:Username"];
var password = builder.Configuration["DBCredentials:Password"];

// SSH and database connection setup
string sshHost = "3.136.81.78";
string sshUsername = "ubuntu";
string sshKeyPath = "C:\\Users\\damon\\Coding_Folder\\APIKeys\\EC2 Instance Key (pem)\\cpsc-team16-key.pem"; //<- to secrets manager later

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
                    .AllowAnyHeader();
            });
    });

    builder.Services.AddDbContext<AppDBContext>(options =>
        options.UseMySql(connection_string,
            new MySqlServerVersion(new Version(8, 0, 25)),
            options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

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

    app.MapControllers();

    app.MapFallbackToFile("/index.html");

    app.Run();

    // Close and clean up the SSH connection
    forwardedPort.Stop();
    sshClient.Disconnect();
}

// builder.Services.AddIdentity<Users, IdentityRole>()
    //     .AddEntityFrameworkStores<AppDBContext>()
    //     .AddDefaultTokenProviders();

    // builder.Services.AddIdentity<Users, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
    //     .AddDefaultTokenProviders();

    // // Add this line to register CustomUserStore
    // builder.Services.AddScoped<IUserStore<Users>, CustomUserStore>();

    // builder.Services.Configure<IdentityOptions>(options =>
    // {
    //     // Password settings
    //     options.Password.RequireDigit = true;
    //     options.Password.RequireLowercase = true;
    //     options.Password.RequireNonAlphanumeric = true;
    //     options.Password.RequireUppercase = true;
    //     options.Password.RequiredLength = 6;
    //     options.Password.RequiredUniqueChars = 1;

    //     // Lockout settings
    //     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    //     options.Lockout.MaxFailedAccessAttempts = 5;
    //     options.Lockout.AllowedForNewUsers = true;

    //     // User settings
    //     options.User.AllowedUserNameCharacters =
    //     "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    //     options.User.RequireUniqueEmail = true;
    // });