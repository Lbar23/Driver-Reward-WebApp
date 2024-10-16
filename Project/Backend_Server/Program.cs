using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Backend_Server;
using Backend_Server.Models;
using Renci.SshNet;
using Amazon;
using Amazon.SecretsManager;
using Amazon.Extensions.NETCore.Setup;
using System.Text.Json;
using Amazon.SecretsManager.Model;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// AWS Secrets Manager setup
var awsOptions = new AWSOptions
{
    Region = RegionEndpoint.USEast2,
};

builder.Services.AddAWSService<IAmazonSecretsManager>(awsOptions);

var secretsManager = builder.Services.BuildServiceProvider().GetRequiredService<IAmazonSecretsManager>();

async Task<string> GetSecret(string secretName)
{
    try
    {
        var response = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT"
        });
        return response.SecretString;
    }
    catch (AmazonSecretsManagerException e)
    {
        Console.WriteLine($"Error retrieving secret: {e.Message}");
        throw;
    }
}

// Retrieving the JSON secret values
var dbSecretStr = await GetSecret("team16/rds-instance/db-credentials");
var dbSecrets = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dbSecretStr);
var sshSecretStr = await GetSecret("team16/ec2-instance/ssh-credentials");
var sshSecrets = JsonSerializer.Deserialize<Dictionary<string, string>>(sshSecretStr);

var username = dbSecrets["username"].GetString();
var password = dbSecrets["password"].GetString();
string sshHost = sshSecrets["host"];
string sshUsername = sshSecrets["username"];
string sshKeyContent = sshSecrets["keypath"];
string dbHost = dbSecrets["host"].GetString();
int dbPort = dbSecrets["port"].GetInt32();

string formattedKey = FormatSshKey(sshKeyContent);

// Write to a temporary file that stores the pem key
var tempKeyPath = Path.GetTempFileName();
File.WriteAllText(tempKeyPath, formattedKey);

try
{
    using (var sshClient = new SshClient(sshHost, sshUsername, new PrivateKeyFile(tempKeyPath)))
    {
        sshClient.Connect();
        var forwardedPort = new ForwardedPortLocal("127.0.0.1", (uint)dbPort, dbHost, (uint)dbPort);
        sshClient.AddForwardedPort(forwardedPort);
        forwardedPort.Start();

        var connection_string = builder.Configuration.GetConnectionString("DefaultConnection")
            .Replace("{Username}", username)
            .Replace("{Password}", password)
            .Replace("{Host}", "127.0.0.1")
            .Replace("{Port}", dbPort.ToString());

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
        builder.Services.AddIdentity<Users, IdentityRole<int>>(options => {
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

        builder.Services.AddDbContext<AppDBContext>(options =>
            options.UseMySql(connection_string,
                ServerVersion.AutoDetect(connection_string),
                options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
        

        // Adds static files to root Backend
        builder.Services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Good Driver Incentive Program API v2"));
        }
        else {
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

        // Close and clean up the SSH connection
        forwardedPort.Stop();
        sshClient.Disconnect();
    }
}
finally
{
    // Temporary file is deleted after use
    if (File.Exists(tempKeyPath))
    {
        File.Delete(tempKeyPath);
    }
}

// Custom function to correctly parse OpenSSH Key file contents
string FormatSshKey(string key)
{
    const int LINE_LENGTH = 64;
    var sb = new StringBuilder();
    sb.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
    
    // Remove any existing formatting
    key = key.Replace("-----BEGIN RSA PRIVATE KEY-----", "")
             .Replace("-----END RSA PRIVATE KEY-----", "")
             .Replace("\n", "")
             .Replace("\r", "");
    
    for (int i = 0; i < key.Length; i += LINE_LENGTH)
    {
        if (i + LINE_LENGTH >= key.Length)
            sb.AppendLine(key.Substring(i));
        else
            sb.AppendLine(key.Substring(i, LINE_LENGTH));
    }
    
    sb.AppendLine("-----END RSA PRIVATE KEY-----");
    return sb.ToString();
}