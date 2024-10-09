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
string sshKeyPath = "../cpsc-team16-key.pem"; //<- to secrets manager later

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

    //Database Connection Setup
    builder.Services.AddDbContext<AppDBContext>(options =>
        options.UseMySql(connection_string,
            new MySqlServerVersion(new Version(8, 0, 25)),
            options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

    //Identity Setup
    builder.Services.AddIdentity<Users, IdentityRole<int>>(options => {
        options.SignIn.RequireConfirmedAccount = false;
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

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
    });

    // CORS configuration and middle ware
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            builder =>
            {
                builder.WithOrigins("http://localhost:5173", "http://localhost:5062")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
    });
    


    // Add SPA services
    builder.Services.AddSpaStaticFiles(configuration =>
    {
        configuration.RootPath = "../team16-webapp-4910.client/dist";
    });

    var app = builder.Build();

    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseSpaStaticFiles();

    app.UseRouting();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1"));
    }

    //app.UseHttpsRedirection();

    app.UseCors("AllowAllOrigins");

    // Each HTTP below can be switched to HTTPS for higher security reasons
    // But at the bare minimum, the Web Application can run on HTTP

    // app.UseCors(policy =>
    //     policy.WithOrigins("http://localhost:5173", "http://localhost:5062")
    //         .AllowAnyHeader()
    //         .AllowAnyMethod()
    // );

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "../team16-webapp-4910.client";
        if (app.Environment.IsDevelopment())
        {
            spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
        }
    });

    app.Run();

    // Close and clean up the SSH connection
    forwardedPort.Stop();
    sshClient.Disconnect();
}