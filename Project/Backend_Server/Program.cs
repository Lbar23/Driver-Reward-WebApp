using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Backend_Server;
using Backend_Server.Models;
using Backend_Server.Infrastructure;
using Backend_Server.Services;
using Amazon;
using Amazon.SecretsManager;
using Amazon.Extensions.NETCore.Setup;
using Serilog;
using Amazon.S3;


var builder = WebApplication.CreateBuilder(args);

//Checks if the current build is in "DesignTime" mode (running dotnet with flags other that run/start/etc)
var isDesignTime = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Development",
    StringComparison.OrdinalIgnoreCase) && 
    AppDomain.CurrentDomain.GetAssemblies().Any(a => a?.FullName?.Contains("Microsoft.EntityFrameworkCore.Design") ?? false);

try {
    // AWS Secrets Manager setup
    var awsOptions = new AWSOptions
    {
        Region = RegionEndpoint.USEast2,
    };

    builder.Services.AddAWSService<IAmazonSecretsManager>(awsOptions);
    builder.Services.AddAWSService<IAmazonS3>(awsOptions);

    builder.Services.AddHttpClient();

    //builder.Services.AddSingleton<CatalogService>();
    builder.Services.AddScoped<DbConnectionProvider>();
    builder.Services.AddScoped<NotifyService>();

    builder.Services.AddControllers();

    // Swagger setup
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Good Driver Incentive Program API", Version = "v2" });
    });

    // CORS setup
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            builder =>
            {
                builder.WithOrigins("BASE_API_URL", "http://localhost:5041")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
    });

    // Identity Services and Options
    builder.Services.AddIdentity<Users, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Tokens.AuthenticatorIssuer = "GitGud";
        options.Tokens.EmailConfirmationTokenProvider = "Email";
        options.Tokens.ChangePhoneNumberTokenProvider = "Phone";
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8; // Increased to 8 characters for regular (Driver) users
        options.Password.RequiredUniqueChars = 1;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;
    })
    .AddPasswordValidator<AdminPasswordValidator>() //I forgot Identity had this, this is hella based
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<PhoneNumberTokenProvider<Users>>("phone")
    .AddTokenProvider<EmailTokenProvider<Users>>("email");

    // Add cookie authentication
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/api/user/login";
        options.LogoutPath = "/api/user/logout"; 
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

    builder.Services.AddDbContext<AppDBContext>((serviceProvider, options) =>
    {
        var dbProvider = serviceProvider.GetRequiredService<DbConnectionProvider>();
        var connection = dbProvider.GetDbConnectionAsync().Result;
        options.UseMySql(connection.ConnectionString,
            ServerVersion.AutoDetect(connection.ConnectionString),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
    });

    // Automated Backup Service
    builder.Services.AddHostedService<BackupService>();

    // Adds static files to root Backend
    builder.Services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");

    if (!isDesignTime)
    {
        // Basic Serilog Service build
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Good Driver Incentive Program API v2"));
    }
    else
    {
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseSpaStaticFiles();
    app.UseCors("AllowSpecificOrigins");
    app.UseRouting();
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });



    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    //SPA route
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "wwwroot";
    });

    app.MapFallbackToFile("/index.html");

    if (!isDesignTime)
    {
        // Enable Serilog Request Logging for API requests
        app.UseSerilogRequestLogging();
        
        Log.Information("Starting Web Host...");
    }

    
    app.Run();
}
catch (Exception ex)
{  
    if (!isDesignTime)
    {
        Log.Fatal(ex, "The web server terminated unexpectedly.");
    }
    throw;
}
finally
{
    if (!isDesignTime)
    {
        Log.CloseAndFlush();
    }
}