// Create new file: LogParserService.cs
using System.Text.RegularExpressions;
using Backend_Server.Infrastructure;
using Backend_Server.Models;
using MySql.Data.MySqlClient;

namespace Backend_Server.Services
{
    public class LogParserService : BackgroundService
    {
        private readonly DbConnectionProvider _connectionProvider;
        private readonly IConfiguration _configuration;
        private readonly string _logDirectory;
        private FileSystemWatcher? _watcher;
        private readonly SemaphoreSlim _processLock = new(1, 1);
        private readonly HashSet<string> _processedEntries = new();

        public LogParserService(
            DbConnectionProvider connectionProvider,
            IConfiguration configuration)
        {
            _connectionProvider = connectionProvider;
            _configuration = configuration;
            _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Wait for initial startup to complete
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            
            await EnsureTableExists();

            _watcher = new FileSystemWatcher(_logDirectory, "latest-*.log")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
            };

            _watcher.Changed += async (sender, e) => await ProcessLogFileWithRetry(e.FullPath);
            _watcher.EnableRaisingEvents = true;

            // Initial processing of existing log files
            foreach (var file in Directory.GetFiles(_logDirectory, "latest-*.log"))
            {
                await ProcessLogFileWithRetry(file);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task ProcessLogFileWithRetry(string filePath, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await _processLock.WaitAsync();
                    try
                    {
                        // Wait for file to be released
                        await Task.Delay(500);

                        string[] lines;
                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var reader = new StreamReader(fileStream))
                        {
                            lines = (await reader.ReadToEndAsync()).Split(Environment.NewLine);
                        }

                        foreach (var line in lines)
                        {
                            // Create a unique identifier for the log entry
                            var entryHash = $"{line.GetHashCode()}";
                            
                            // Skip if we've already processed this entry
                            if (_processedEntries.Contains(entryHash))
                                continue;

                            var logEntry = ParseLogLine(line);
                            if (logEntry != null)
                            {
                                await SaveToDatabaseWithRetry(logEntry);
                                _processedEntries.Add(entryHash);
                                
                                // Keep the set from growing too large
                                if (_processedEntries.Count > 10000)
                                {
                                    _processedEntries.Clear();
                                }
                            }
                        }
                        break; // Success, exit retry loop
                    }
                    finally
                    {
                        _processLock.Release();
                    }
                }
                catch (IOException) when (i < maxRetries - 1)
                {
                    await Task.Delay(1000 * (i + 1)); // Exponential backoff
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing log file (attempt {i + 1}/{maxRetries}): {ex.Message}");
                    if (i == maxRetries - 1) throw;
                }
            }
        }

        private async Task SaveToDatabaseWithRetry(AuditLogs log, int maxRetries = 3)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using var connection = await _connectionProvider.GetDbConnectionAsync();
                    await connection.OpenAsync();

                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                        INSERT IGNORE INTO AuditLogs (UserID, Category, Description, Timestamp) 
                        VALUES (@UserID, @Category, @Description, @Timestamp)";

                    cmd.Parameters.AddWithValue("@UserID", log.UserID);
                    cmd.Parameters.AddWithValue("@Category", log.Category.ToString());
                    cmd.Parameters.AddWithValue("@Description", log.Description);
                    cmd.Parameters.AddWithValue("@Timestamp", log.Timestamp);

                    await cmd.ExecuteNonQueryAsync();
                    return; // Success
                }
                catch (Exception ex) when (i < maxRetries - 1)
                {
                    Console.WriteLine($"Error saving to database (attempt {i + 1}/{maxRetries}): {ex.Message}");
                    await Task.Delay(1000 * (i + 1)); // Exponential backoff
                }
            }
        }

        private AuditLogs? ParseLogLine(string line)
        {
            try
            {
                var match = Regex.Match(line, @"(\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}.\d+).*?\[INF\]\s+(.+)");
                if (!match.Success) return null;

                var timestamp = DateTime.Parse(match.Groups[1].Value);
                var message = match.Groups[2].Value;

                var userIdMatch = Regex.Match(message, @"UserID:\s*(\d+|N/A)");
                var categoryMatch = Regex.Match(message, @"Category:\s*(\w+)");
                var descriptionMatch = Regex.Match(message, @"Description:\s*(.+)$");

                if (!userIdMatch.Success || !categoryMatch.Success || !descriptionMatch.Success)
                    return null;

                // Handle "N/A" UserID
                var userIdStr = userIdMatch.Groups[1].Value;
                var userId = userIdStr == "N/A" ? 0 : int.Parse(userIdStr);
                
                // Parse the category, defaulting to System if invalid
                if (!Enum.TryParse<AuditLogCategory>(categoryMatch.Groups[1].Value, out var category))
                    category = AuditLogCategory.System;

                return new AuditLogs
                {
                    UserID = userId,
                    Category = category,
                    Description = descriptionMatch.Groups[1].Value.Trim(),
                    Timestamp = timestamp
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task SaveToDatabase(AuditLogs log)
        {
            try
            {
                using var connection = await _connectionProvider.GetDbConnectionAsync();
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO AuditLogs (UserID, Category, Description, Timestamp) 
                    VALUES (@UserID, @Category, @Description, @Timestamp)
                    ON DUPLICATE KEY UPDATE 
                        UserID = VALUES(UserID),
                        Category = VALUES(Category),
                        Description = VALUES(Description)";

                cmd.Parameters.AddWithValue("@UserID", log.UserID);
                cmd.Parameters.AddWithValue("@Category", log.Category.ToString());
                cmd.Parameters.AddWithValue("@Description", log.Description);
                cmd.Parameters.AddWithValue("@Timestamp", log.Timestamp);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to database: {ex.Message}");
            }
        }

        private async Task EnsureTableExists()
        {
            try
            {
                using var connection = await _connectionProvider.GetDbConnectionAsync();
                await connection.OpenAsync();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS AuditLogs (
                        LogID INT AUTO_INCREMENT PRIMARY KEY,
                        UserID INT NOT NULL,
                        Category VARCHAR(50) NOT NULL,
                        Description TEXT,
                        Timestamp DATETIME NOT NULL,
                        UNIQUE KEY unique_log (Timestamp, UserID, Category)
                    )";

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _watcher?.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
}