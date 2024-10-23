using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Backend_Server.Infrastructure;
using MySql.Data.MySqlClient;
// using Amazon.S3;
// using Amazon.S3.Model;



/// <summary>
/// Better documentation of each class that's made; Similar to JavaDocs
/// This is just a simple implementation of automated backup database service. 
/// Delagated to store to local directoryfile, but will be updated to store in S3 bucket storage instance
/// </summary>
namespace Backend_Server.Services
{
    public class BackupService(IConfiguration configuration, IAmazonSecretsManager amazonSecrets) : BackgroundService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IAmazonSecretsManager _amazonSecrets = amazonSecrets;
        private readonly string _backupPath = "var/backups/database";
        
        //add S3 bucket later

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // while (!stoppingToken.IsCancellationRequested)
            // {
            //     Directory.CreateDirectory(_backupPath); // <-- creates directory path; gitignore already configured for it. Remove later when S3...
            //     await BackupDatabase();
            //     //await UploadBackupS3();
            //     await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            // }
        }

        //Basic setup for backup using MySqlBackup
        // private async Task BackupDatabase()
        // {
        //     string backupFile = Path.Combine(_backupPath, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");

        //     using var connectionProvider = new DbConnectionProvider(_configuration, _amazonSecrets);
        //     using var connection = await connectionProvider.GetDbConnectionAsync();
        //     using MySqlCommand cmd = new();
        //     using MySqlBackup mb = new(cmd);
        //     cmd.Connection = connection;
        //     await connection.OpenAsync();
        //     mb.ExportToFile(backupFile);
        //     // S3 Bucket Storage addition
        // }

        // // Upload to S3 Bucket implementation later
        private async Task UploadBackupS3() {
            
        }
    }
}