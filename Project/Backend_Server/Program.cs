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
using Serilog.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static Backend_Server.Services.ClaimsService;

var builder = WebApplication.CreateBuilder(args);

var isDesignTime = string.Equals(
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
    "Development",
    StringComparison.OrdinalIgnoreCase) && 
    AppDomain.CurrentDomain.GetAssemblies().Any(a => a?.FullName?.Contains("Microsoft.EntityFrameworkCore.Design") ?? false);

try {
    var awsOptions = new AWSOptions
    {
        Region = RegionEndpoint.USEast2,
    };

    builder.Services.AddAWSService<IAmazonSecretsManager>(awsOptions);
    builder.Services.AddAWSService<IAmazonS3>(awsOptions);

    builder.Services.AddHttpClient();

    builder.Services.AddSingleton<CatalogService>();
    builder.Services.AddSingleton<DbConnectionProvider>();
    builder.Services.AddScoped<NotifyService>();
    builder.Services.AddScoped<ReportService>();
    builder.Services.AddScoped<ClaimsService>();

    builder.Services.AddLogging(configure => {
        configure.AddConsole();
        configure.AddDebug();
        configure.SetMinimumLevel(LogLevel.Information);
    });
    
    builder.Services.AddMemoryCache();

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Good Driver Incentive Program API", Version = "v2" });
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigins",
            builder =>
            {
                builder.WithOrigins("http://localhost:5173", "http://localhost:5041", "http://localhost:5000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
    });

    // Generate random key for JWT
    byte[] key = new byte[32];
    using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
    {
        rng.GetBytes(key);
    }

    // Add the key to DI container
    builder.Services.AddSingleton<byte[]>(key);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("JwtBearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = "GitGud",  
            ValidateAudience = true,
            ValidAudience = "GitGud",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["X-Access-Token"];
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(PolicyNames.CanImpersonate, policy =>
            policy.RequireRole("Admin"));
            
        options.AddPolicy(PolicyNames.RequireSponsorRole, policy =>
            policy.RequireRole("Sponsor"));
            
        options.AddPolicy(PolicyNames.RequireAdminRole, policy =>
            policy.RequireRole("Admin"));
    
        options.AddPolicy("CanCreateUsers", policy =>
            policy.RequireRole("Admin")
                  .RequireClaim("Permission", "CreateUsers"));
    });

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
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;
    })
    .AddPasswordValidator<UserPasswordValidator>()
    .AddEntityFrameworkStores<AppDBContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<PhoneNumberTokenProvider<Users>>("phone")
    .AddTokenProvider<EmailTokenProvider<Users>>("email");

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/api/system/login";
        options.LogoutPath = "/api/system/logout"; 
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

    builder.Services.AddDbContext<AppDBContext>((serviceProvider, options) =>
    {
        var dbProvider = serviceProvider.GetRequiredService<DbConnectionProvider>();
        var connection = dbProvider.GetDbConnectionAsync().Result;
        options.UseMySql(connection.ConnectionString,
            new MySqlServerVersion(new Version(8, 0, 28)),
            mySqlOptions => 
            {
                mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                mySqlOptions.CommandTimeout(30);
                mySqlOptions.EnableStringComparisonTranslations();
                mySqlOptions.MigrationsAssembly("Backend_Server");
            });
    });

    builder.Services.AddSingleton<IHostedService, BackupService>();

    builder.Services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");

    if (!isDesignTime)
    {
        try 
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to configure Serilog: {ex.Message}");
            throw;
        }
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
        app.UseHttpsRedirection();
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

    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "wwwroot";
    });

    app.MapFallbackToFile("/index.html");

    if (!isDesignTime)
    {
        app.UseSerilogRequestLogging();
        Log.Information("UserID: N/A, Category: System, Description: Starting Web Host...");
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