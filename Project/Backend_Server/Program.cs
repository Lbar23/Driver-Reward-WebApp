using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Backend_Server;
using Backend_Server.Models;
using Backend_Server.Infrastructure;
using Backend_Server.Services;
using Amazon;
using Amazon.SecretsManager;
using Amazon.Extensions.NETCore.Setup;

var builder = WebApplication.CreateBuilder(args);

// AWS Secrets Manager setup
var awsOptions = new AWSOptions
{
    Region = RegionEndpoint.USEast2,
};

builder.Services.AddAWSService<IAmazonSecretsManager>(awsOptions);

builder.Services.AddScoped<DbConnectionProvider>();

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
                .AllowAnyHeader();
        });
});

// Identity Services and Options
builder.Services.AddIdentity<Users, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Tokens.AuthenticatorIssuer = "GitGud";
    options.Tokens.EmailConfirmationTokenProvider = "email";
    options.Tokens.ChangePhoneNumberTokenProvider = "phone";
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

// Add cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    // options.LoginPath = "/api/login";  //will come back to later
    // options.LogoutPath = "/api/logout"; 
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

app.UseRouting();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//SPA route
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";
});

app.MapFallbackToFile("/index.html");

app.Run();