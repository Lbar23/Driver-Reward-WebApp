using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend_Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backend_Server.Services
{
    public class LoggingService(UserManager<Users> userManager, AppDBContext appDBContext)
    {
        private readonly UserManager<Users> _userManager = userManager;
        private readonly AppDBContext _appDBContext = appDBContext;

        public async Task LogAuthenticationAsync(int userId, AuthenticationType authType, bool success, string? userAgent, string? details)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException("User not found.");
            var authentication = new Authentications
            {
                UserID = userId,
                User = user,
                AuthType = authType,
                Success = success,
                Timestamp = DateTime.UtcNow,
                UserAgent = userAgent,
                Details = details
            };

            _appDBContext.Authentications.Add(authentication);
            await _appDBContext.SaveChangesAsync();
        }

        public async Task LogAccountActivityAsync(int userId, ActivityType activityType, string? details)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException("User not found.");
            var accountActivity = new AccountActivity
            {
                UserId = userId,
                User = user,
                Timestamp = DateTime.UtcNow,
                ActivityType = activityType,
                Details = details
            };

            _appDBContext.AccountActivity.Add(accountActivity);
            await _appDBContext.SaveChangesAsync();
        }

        public async Task LogAuditAsync(int userId, AuditLogCategory category, AuditLogAction action, bool success, string? details)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new InvalidOperationException("User not found.");
            var auditLog = new AuditLogs
            {
                UserID = userId,
                User = user,
                Category = category,
                Action = action,
                ActionSuccess = success,
                Timestamp = DateTime.UtcNow,
                AdditionalDetails = details
            };

            _appDBContext.AuditLogs.Add(auditLog);
            await _appDBContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Authentications>> GetUserAuthenticationLogsAsync(int userId)
        {
            return await _appDBContext.Authentications
                .Where(a => a.UserID == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<IEnumerable<AccountActivity>> GetUserAccountActivityLogsAsync(int userId)
        {
            return await _appDBContext.AccountActivity
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }
    }
}