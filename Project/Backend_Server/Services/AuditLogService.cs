using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Backend_Server.Services
{
    public class AuditLogFilter
    {
        public int? UserID { get; set; }
        public AuditLogCategory? Category { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AuditLogResult
    {
        public List<AuditLog> Logs { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public interface IAuditLogService
    {
        Task<AuditLogResult> GetAuditLogsAsync(AuditLogFilter filter);
        Task<List<AuditLog>> GetUserAuditLogsAsync(int userId);
        Task<List<AuditLog>> GetCategoryAuditLogsAsync(AuditLogCategory category);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLogResult> GetAuditLogsAsync(AuditLogFilter filter)
        {
            var query = _context.AuditLogs.AsQueryable();

            // Apply filters
            if (filter.UserID.HasValue)
            {
                query = query.Where(log => log.UserID == filter.UserID.Value);
            }

            if (filter.Category.HasValue)
            {
                query = query.Where(log => log.Category == filter.Category.Value);
            }

            if (filter.StartDate.HasValue)
            {
                query = query.Where(log => log.Timestamp >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(log => log.Timestamp <= filter.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(log => log.Description.Contains(filter.SearchTerm));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);

            // Apply pagination
            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new AuditLogResult
            {
                Logs = logs,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = filter.Page
            };
        }

        public async Task<List<AuditLog>> GetUserAuditLogsAsync(int userId)
        {
            return await _context.AuditLogs
                .Where(log => log.UserID == userId)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetCategoryAuditLogsAsync(AuditLogCategory category)
        {
            return await _context.AuditLogs
                .Where(log => log.Category == category)
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
    }
}