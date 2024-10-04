using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Renci.SshNet;


var builder = WebApplication.CreateBuilder(args);

// SSH Client var

string SSHHost = "3.136.81.78";
string SSHUsername = "ubuntu";
string keyPath = "C:\\Users\\ragas\\Downloads\\cpsc-team16-key.pem";

// Host database connection

string dbHost = "team16-database.cpin0o6jvads.us-east-2.rds.amazonaws.com";
int dbPort = 3306;

using (var sshClient = new SshClient(SSHHost, SSHUsername, new PrivateKeyFile(keyPath))) {

    sshClient.Connect();
    var forwardedPort = new ForwardedPortLocal("127.0.0.1", (uint)dbPort, dbHost, (uint)dbPort);
    sshClient.AddForwardedPort(forwardedPort);
    forwardedPort.Start();

    var connection_string = builder.Configuration.GetConnectionString("DefaultConnection")
        // .Replace("{Username}", username)
        // .Replace("{Password}", password)
        .Replace(dbHost, "127.0.0.1");

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString(connection_string),
    new MySqlServerVersion(new Version(8, 0, 21))));
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
options.AddPolicy("AllowReactApp",
    builder => builder.WithOrigins("http://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

try
{
using var connection = new MySqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"));
connection.Open();
Console.WriteLine("Database connection successful!");
}
catch (Exception ex)
{
Console.WriteLine($"Database connection failed: {ex.Message}");
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");


app.UseAuthorization();

app.MapControllers();

app.Run();

forwardedPort.Stop();
sshClient.Disconnect();

}