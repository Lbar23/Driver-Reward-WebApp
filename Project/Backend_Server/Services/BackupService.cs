using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Backend_Server.Infrastructure;
using MySql.Data.MySqlClient;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Serilog;
using Amazon.S3.Util;



/// <summary>
/// Better documentation of each class that's made; Similar to JavaDocs
/// This is just a simple implementation of automated backup database service. 
/// Delagated to store to local directoryfile, but will be updated to store in S3 bucket storage instance
/// </summary>
namespace Backend_Server.Services
{
    public class BackupService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IAmazonSecretsManager _amazonSecrets;
        private readonly IAmazonS3 _s3Client;
        private readonly string _backupPath;
        private readonly string _bucketName;
        private const int BACKUP_RETENTION_DAYS = 7;

        public BackupService(
            IConfiguration configuration, 
            IAmazonSecretsManager amazonSecrets,
            IAmazonS3 s3Client)
        {
            _configuration = configuration;
            _amazonSecrets = amazonSecrets;
            _s3Client = s3Client;
            _backupPath = "var/backups/database";
            _bucketName = _configuration["AWS:BackupBucketName"] ?? "team16-db-backups";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Directory.CreateDirectory(_backupPath);

                    var backupFile = await BackupDatabase();

                    await UploadBackupToS3(backupFile);

                    await CleanupOldBackups();

                    if (File.Exists(backupFile)) {
                        File.Delete(backupFile);
                    }

                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during database backup process");
                    Log.Information("This is normal for right now; configure implementation to end task normally later; server doesn't really need that anyway");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); //Retry after 30 mins
                }
            }
        }

        private async Task<string> BackupDatabase()
        {
            string backupFile = Path.Combine(_backupPath, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");

            using var connectionProvider = new DbConnectionProvider(_configuration, _amazonSecrets);
            using var connection = await connectionProvider.GetDbConnectionAsync();
            using var cmd = new MySqlCommand { Connection = connection };
            using var mb = new MySqlBackup(cmd);
            
            await connection.OpenAsync();
            mb.ExportToFile(backupFile);

            return backupFile;
        }

        private async Task UploadBackupToS3(string backupFile)
        {
            try
            {
                // Ensure bucket exists
                var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
                if (!bucketExists)
                {
                    await _s3Client.PutBucketAsync(new PutBucketRequest
                    {
                        BucketName = _bucketName,
                        UseClientRegion = true
                    });
                }

                // Upload file
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
                {
                    FilePath = backupFile,
                    BucketName = _bucketName,
                    Key = Path.GetFileName(backupFile),
                    // Add server-side encryption
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                });

                Log.Information("Successfully uploaded backup to S3: {FileName}", Path.GetFileName(backupFile));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upload backup to S3: {FileName}", Path.GetFileName(backupFile));
                throw;
            }
        }

        private async Task CleanupOldBackups()
        {
            try
            {
                //This deletes local backups; this can be edited locally to delete every backup
                //Or more than one instance...just create a new const variable
                var localFiles = Directory.GetFiles(_backupPath)
                    .Where(f => File.GetCreationTime(f) < DateTime.Now.AddDays(-BACKUP_RETENTION_DAYS));
                foreach (var file in localFiles)
                {
                    File.Delete(file);
                }

                //This deletes old S3 database backups
                //Do not change this at all
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName
                };

                var response = await _s3Client.ListObjectsV2Async(listRequest);
                var oldObjects = response.S3Objects
                    .Where(obj => obj.LastModified < DateTime.UtcNow.AddDays(-BACKUP_RETENTION_DAYS))
                    .ToList();

                foreach (var oldObject in oldObjects)
                {
                    await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = oldObject.Key
                    });
                }

                Log.Information("Cleanup completed. Removed {Count} old backups", oldObjects.Count);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during backup cleanup");
                throw;
            }
        }
    }
}