using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Backend_Server.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Claims;

namespace Backend_Server.Infrastructure
{
    [Authorize(Roles = "Admin")]
    public class TerminalHub : Hub
    {
        private readonly AppDBContext _context;
        private readonly UserManager<Users> _userManager;

        public TerminalHub(
            AppDBContext context,
            UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            try 
            {
                var user = Context.User;
                if (user != null)
                {
                    var dbUser = await _userManager.GetUserAsync(user);
                    Log.Information(
                        "UserID: {UserID}, Category: {Category}, Description: {Description}",
                        dbUser?.Id ?? 0,
                        "System",
                        $"Admin terminal connection established for {dbUser?.UserName ?? "Unknown"}"
                    );
                    // Send immediate acknowledgment to client
                    await Clients.Caller.SendAsync("ReceiveOutput", "Connection established successfully.");
                }
                else
                {
                    Log.Warning("UserID: N/A, Category: {Category}, Description: {Description}", 
                        "Security", 
                        $"Unauthenticated terminal connection attempt from {Context.ConnectionId}");
                    throw new HubException("Authentication required");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in OnConnectedAsync");
                throw;
            }
            await base.OnConnectedAsync();
        }

        public async Task ExecuteCommand(string command)
        {
            if (Context.User == null || !Context.User.Identity.IsAuthenticated)
            {
                Log.Warning(
                    "UserID: N/A, Category: {Category}, Description: {Description}",
                    "System",
                    "Unauthenticated terminal command attempt"
                );
                await Clients.Caller.SendAsync("ReceiveError", "User not authenticated");
                return;
            }

            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null)
            {
                Log.Warning(
                    "UserID: N/A, Category: {Category}, Description: {Description}",
                    "System",
                    "User not found for terminal command attempt"
                );
                await Clients.Caller.SendAsync("ReceiveError", "User not found");
                return;
            }

            // Add debug logging
            Log.Information(
                "UserID: {UserID}, Category: {Category}, Description: {Description},",
                user.Id,
                "System",
                $"Executing terminal command: {command}"
            );

            try
            {
                var (cmd, args) = ParseCommand(command);
                var response = await HandleCommand(cmd, args, user);
                await Clients.Caller.SendAsync("ReceiveOutput", response);

                Log.Information(
                    "UserID: {UserID}, Category: {Category}, Description: {Description}",
                    user.Id,
                    "System",
                    $"Successfully executed command: {command}"
                );
            }
            catch (Exception ex)
            {
                Log.Error(
                    ex,
                    "UserID: {UserID}, Category: {Category}, Description: {Description}",
                    user.Id,
                    "System",
                    $"Error executing command: {command}"
                );
                await Clients.Caller.SendAsync("ReceiveError", $"Error: {ex.Message}");
            }
        }

        private (string cmd, string[] args) ParseCommand(string command)
        {
            var parts = command.Split(' ');
            return (parts[0].ToLower(), parts.Skip(1).ToArray());
        }

        private async Task<string> HandleCommand(string cmd, string[] args, Users user)
        {
            return cmd switch
            {
                "help" => GetHelpText(),
                "status" => await GetSystemStatus(user),
                "metrics" => await GetSystemMetrics(user),
                "users" => await GetRecentUserActivities(user),
                "clear" => string.Empty,
                _ => $"Unknown command: {cmd}. Type 'help' for available commands."
            };
        }

        private string GetHelpText()
        {
            return @"Available Commands:
  help    - Show this help message
  status  - Show system status and active users
  metrics - Show system metrics and statistics
  users   - List recent user activities
  clear   - Clear the terminal screen";
        }

         public async Task BroadcastSystemAlert(string message)
    {
        await Clients.All.SendAsync("SystemAlert", message);
    }

    public async Task BroadcastSystemUpdate(string message)
    {
        await Clients.All.SendAsync("SystemUpdate", message);
    }

    private async Task<string> GetSystemStatus(Users user)
    {
        try
        {
            var activeUsers = await _context.Users
                .Where(u => u.LastLogin >= DateTime.UtcNow.AddMinutes(-15))
                .CountAsync();

            // Broadcast system alert for high user count
            if (activeUsers > 100)
            {
                await BroadcastSystemAlert($"High user activity detected: {activeUsers} active users");
            }

            var output = new StringBuilder()
                .AppendLine("*****")
                .AppendLine("┌──────────── System Status ────────────┐")
                .AppendLine($"│ Active Users: {activeUsers.ToString().PadRight(23)} │")
                .AppendLine($"│ Server Time: {DateTime.UtcNow:HH:mm:ss UTC}".PadRight(37) + "│")
                .AppendLine($"│ Status: {"Operational".PadRight(26)} │")
                .AppendLine("└─────────────────────────────────────┘")
                .ToString();

            return output;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving system status");
            throw;
        }
    }

    private async Task<string> GetSystemMetrics(Users user)
    {
        try
        {
            var userCounts = await _context.Users
                .GroupBy(u => u.UserType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            var process = System.Diagnostics.Process.GetCurrentProcess();
            var memoryUsage = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2);
            var cpuTime = process.TotalProcessorTime;

            // Format metrics with ASCII art bars
            var output = new StringBuilder("System Metrics:\n\n");
            
            foreach (var count in userCounts)
            {
                var percentage = count.Count / (float)userCounts.Sum(x => x.Count) * 100;
                var bars = new string('█', (int)(percentage / 2));
                output.AppendLine($"{count.Type,-10} [{bars,-50}] {count.Count,5} ({percentage:F1}%)");
            }

            output.AppendLine("\nSystem Resources:")
                .AppendLine($"Memory Usage: {memoryUsage:F2}MB")
                .AppendLine($"CPU Time: {cpuTime.TotalSeconds:F1}s")
                .AppendLine($"Uptime: {GetUptime()}");

            // Broadcast updates for high resource usage
            if (memoryUsage > 1000)
            {
                await BroadcastSystemAlert($"High memory usage detected: {memoryUsage:F2}MB");
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving system metrics");
            throw;
        }
    }

    //It won't show ALL users unless they are highly active (more than 24 hours)
    private async Task<string> GetRecentUserActivities(Users user)
    {
        try
        {
            var recentUsers = await _context.Users
                .Where(u => u.LastLogin != null)
                .OrderByDescending(u => u.LastLogin)
                .Take(5)
                .Select(u => new {
                    u.UserName,
                    u.UserType,
                    u.LastLogin,
                    u.NotifyPref,
                    u.TwoFactorEnabled
                })
                .ToListAsync();

            var output = new StringBuilder("Recent User Activities:\n\n");
            foreach (var u in recentUsers)
            {
                var timeAgo = DateTime.UtcNow - u.LastLogin!.Value;
                string timeAgoStr = timeAgo.TotalMinutes switch
                {
                    < 1 => "just now",
                    < 60 => $"{(int)timeAgo.TotalMinutes}m ago",
                    < 1440 => $"{(int)timeAgo.TotalHours}h ago",
                    _ => $"{(int)timeAgo.TotalDays}d ago"
                };

                output.AppendLine($"► {u.UserName} ({u.UserType})")
                    .AppendLine($"  └─ Last seen: {timeAgoStr}")
                    .AppendLine($"  └─ 2FA: {(u.TwoFactorEnabled ? "✓" : "✗")} ({u.NotifyPref})")
                    .AppendLine();
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving user activities");
            throw;
        }
    }

        private string GetUptime()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var uptime = DateTime.Now - process.StartTime;
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }

        private async Task<int> Get2FAEnabledCount()
        {
            return await _context.Users.CountAsync(u => u.TwoFactorEnabled);
        }
    }
}