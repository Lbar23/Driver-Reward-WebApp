using System.Data;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Amazon.SecretsManager;
using Backend_Server.Infrastructure;
using MySqlConnector;
using Serilog;

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
        private readonly CancellationTokenSource _emergencyStopToken = new();

        public BackupService(
            IConfiguration configuration,
            IAmazonSecretsManager amazonSecrets,
            IAmazonS3 s3Client,
            IServiceScopeFactory scopeFactory)
        {
            _configuration = configuration;
            _amazonSecrets = amazonSecrets;
            _s3Client = s3Client;

            _backupPath = "var/backups/database";
            _bucketName = _configuration["AWS:BackupBucketName"] ?? "team16-db-backups";
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken, _emergencyStopToken.Token);

            while (!combinedToken.Token.IsCancellationRequested)
            {
                try
                {
                    Directory.CreateDirectory(_backupPath);

                    var backupFile = await BackupDatabaseAsync();

                    if (combinedToken.Token.IsCancellationRequested && File.Exists(backupFile))
                    {
                        File.Delete(backupFile);
                        break;
                    }

                    await UploadBackupToS3Async(backupFile);
                    await CleanupOldBackupsAsync();

                    if (File.Exists(backupFile))
                    {
                        File.Delete(backupFile);
                    }

                    await Task.Delay(TimeSpan.FromDays(1), combinedToken.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Log.Error(ex, "Error during database backup process");
                    await Task.Delay(TimeSpan.FromMinutes(30), combinedToken.Token);
                }
            }
        }

        private async Task<string> BackupDatabaseAsync()
        {
            var backupFile = Path.Combine(_backupPath, $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.sql");

            // Use `using` instead of `await using` because `DbConnectionProvider` does not implement `IAsyncDisposable`
            using var connectionProvider = new DbConnectionProvider(_configuration, _amazonSecrets);
            var connection = await connectionProvider.GetDbConnectionAsync();

            // Ensure the connection is open
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            await using var cmd = connection.CreateCommand();
            var mb = new MySqlBackup(cmd)
            {
                ExportInfo =
                {
                    ExportRows = true,
                    RecordDumpTime = true
                }
            };

            try
            {
                await using var sessionCmd = connection.CreateCommand();
                sessionCmd.CommandText = @"
                    SET SESSION time_zone='+00:00';
                    SET SESSION sql_mode='ALLOW_INVALID_DATES,NO_ZERO_DATE';
                ";
                await sessionCmd.ExecuteNonQueryAsync();

                // Perform the database export
                mb.ExportToFile(backupFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to perform database backup");
                throw;
            }

            return backupFile;
        }

        private async Task UploadBackupToS3Async(string backupFile)
        {
            try
            {
                if (!await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName))
                {
                    await _s3Client.PutBucketAsync(new PutBucketRequest
                    {
                        BucketName = _bucketName,
                        UseClientRegion = true
                    });
                }

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(new TransferUtilityUploadRequest
                {
                    FilePath = backupFile,
                    BucketName = _bucketName,
                    Key = Path.GetFileName(backupFile),
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
                });

                Log.Information("Backup uploaded to S3: {FileName}", Path.GetFileName(backupFile));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to upload backup to S3: {FileName}", Path.GetFileName(backupFile));
                throw;
            }
        }

        private async Task CleanupOldBackupsAsync()
        {
            try
            {
                var localFiles = Directory.GetFiles(_backupPath)
                    .Where(f => File.GetCreationTime(f) < DateTime.UtcNow.AddDays(-BACKUP_RETENTION_DAYS));
                foreach (var file in localFiles)
                {
                    File.Delete(file);
                }

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

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Log.Information("Backup service is stopping...");
            _emergencyStopToken.Cancel();
            await base.StopAsync(cancellationToken);
            Log.Information("Backup service stopped successfully");
        }
    }
}
