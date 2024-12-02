using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Backend_Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Backend_Server.Services
{
    public class ClaimsService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<ClaimsService> _logger;
        private readonly IMemoryCache _cache;
        private readonly AppDBContext _context;

        public ClaimsService(
            IConfiguration configuration,
            UserManager<Users> userManager,
            ILogger<ClaimsService> logger,
            AppDBContext context,
            IMemoryCache cache)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        public static class CustomClaimTypes
        {
            public const string NotificationPreferences = "notification_prefs";
            public const string LastLoginDate = "last_login_date";
            public const string FullName = "full_name";
            public const string SponsorInfo = "sponsor_info";
            public const string ImpersonatorId = "impersonator_id";
            public const string OriginalUserId = "original_user_id";
            public const string SystemPreferences = "system_prefs";
        }

        public static class PolicyNames
        {
            public const string RequireAdminRole = "RequireAdminRole";
            public const string CanImpersonate = "CanImpersonate";
            public const string RequireSponsorRole = "RequireSponsorRole";
            public const string RequireSystemPreferences = "RequireSystemPreferences";
        }


        public async Task<string> GenerateJwtToken(Users user, IList<Claim>? additionalClaims = null)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Add roles as claims
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Add custom claims
            claims.AddRange(userClaims);

            // Add additional claims if provided
            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ??
                throw new InvalidOperationException("JWT secret key is not configured")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool isValid, Users? user)> ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return (false, null);

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = _configuration["Jwt:SecretKey"];

                if (string.IsNullOrWhiteSpace(secretKey)){
                    throw new InvalidOperationException("JWT secret key is not configured");
                }
                var key = Encoding.UTF8.GetBytes(secretKey);


                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return (false, null);

                var user = await _userManager.FindByIdAsync(userId);
                return (true, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
                return (false, null);
            }
        }

        public async Task<IList<Claim>> GetUserClaims(Users user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(CustomClaimTypes.LastLoginDate, user.LastLogin?.ToString("O") ?? DateTime.UtcNow.ToString("O"))
            };

            // Add notification preferences
            var notifyPrefs = new
            {
                email = user.NotifyPref == NotificationPref.Email,
                phone = user.NotifyPref == NotificationPref.Phone,
                twoFactorEnabled = user.TwoFactorEnabled
            };

            claims.Add(new Claim(CustomClaimTypes.NotificationPreferences,
                System.Text.Json.JsonSerializer.Serialize(notifyPrefs)));

            // Add role information
            string? roleName = user.Role?.Name ?? await GetRoleNameAsync(user.Role.Id);
            if (!string.IsNullOrEmpty(roleName))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            // Add type-specific claims
            claims.AddRange(GenerateTypeSpecificClaims(user, roleName));

            // Add sponsor info for sponsor users
            if (roleName == UserType.Sponsor.ToString())
            {
                var sponsorInfo = await GetSponsorInfo(user.Id);
                if (sponsorInfo != null)
                {
                    claims.Add(new Claim(CustomClaimTypes.SponsorInfo,
                        System.Text.Json.JsonSerializer.Serialize(sponsorInfo)));
                }
            }

            return claims;
        }

        private List<Claim> GenerateTypeSpecificClaims(Users user, string roleName)
        {
            return roleName switch
            {
                "Admin" => new List<Claim>
                {
                    new Claim("CanImpersonate", "true"),
                    new Claim("AdminCreatedAt", DateTime.UtcNow.ToString("O"))
                },
                "Sponsor" => new List<Claim>
                {
                    new Claim("PointDollarValue", "0.01"),
                    new Claim("IsPrimary", "true")
                },
                "Driver" => new List<Claim>
                {
                    new Claim("DriverStatus", "Active"),
                    new Claim("DriverCreatedAt", DateTime.UtcNow.ToString("O"))
                },
                _ => new List<Claim>()
            };
        }

        public async Task UpdateUserClaims(Users user)
        {
            try
            {
                var existingClaims = await _userManager.GetClaimsAsync(user);

                // Remove all non-core claims
                var nonCoreClaims = existingClaims.Where(c => !IsCoreClaim(c.Type)).ToList();
                if (nonCoreClaims.Any())
                {
                    await _userManager.RemoveClaimsAsync(user, nonCoreClaims);
                }

                // Generate updated claims
                var newClaims = await GetUserClaims(user);
                await _userManager.AddClaimsAsync(user, newClaims);
            }

            catch (Exception ex){
                _logger.LogError(ex, "Error updating claims for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<string?> GetRoleNameAsync(int roleId)
        {
            var cacheKey = $"RoleName_{roleId}";
            if (_cache.TryGetValue(cacheKey, out string? roleName))
            {
                return roleName;
            }

            roleName = await _context.Roles
                .Where(r => r.Id == roleId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync();

            if (roleName != null)
            {
                _cache.Set(cacheKey, roleName, TimeSpan.FromHours(1)); // Cache for 1 hour
            }

            return roleName;
        }
        public bool IsCoreClaim(string claimType)
        {
            var coreClaims = new[]
            {
                ClaimTypes.NameIdentifier,
                ClaimTypes.Name,
                ClaimTypes.Email,
                ClaimTypes.Role
            };

            return coreClaims.Contains(claimType);
        }

        public async Task<IList<Claim>> SetImpersonation(Users impersonator, Users targetUser)
        {
            var claims = await GetUserClaims(targetUser);

            // Add impersonation claims
            claims.Add(new Claim(CustomClaimTypes.ImpersonatorId, impersonator.Id.ToString()));
            claims.Add(new Claim(CustomClaimTypes.OriginalUserId, targetUser.Id.ToString()));

            return claims;
        }

        public async Task<bool> RemoveImpersonation(Users user)
        {
            try
            {
                var claims = await _userManager.GetClaimsAsync(user);
                var impersonationClaims = claims.Where(c =>
                    c.Type == CustomClaimTypes.ImpersonatorId ||
                    c.Type == CustomClaimTypes.OriginalUserId).ToList();

                foreach (var claim in impersonationClaims)
                {
                    await _userManager.RemoveClaimAsync(user, claim);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing impersonation for user {UserId}", user.Id);
                return false;
            }
        }

        private async Task<object?> GetSponsorInfo(int userId)
        {
            try
            {
                return await _context.SponsorUsers
                    .Include(su => su.Sponsor)
                    .Where(su => su.UserID == userId)
                    .Select(su => new
                    {
                        su.SponsorID,
                        su.Sponsor.CompanyName,
                        su.IsPrimary,
                        su.JoinDate
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sponsor info for user {UserId}", userId);
                return null;
            }
        }

        public async Task CreateAuditLog(
            int userId,
            AuditLogCategory category,
            AuditLogAction action,
            bool actionSuccess,
            string? additionalDetails = null)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    Log.Error("Failed to create audit log - User not found: {UserId}", userId);
                    return;
                }

                var auditLog = new AuditLogs
                {
                    UserID = userId,
                    User = user,
                    Category = category,
                    Action = action,
                    ActionSuccess = actionSuccess,
                    Timestamp = DateTime.UtcNow,
                    AdditionalDetails = additionalDetails
                };

                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();

                Log.Information(
                    "Audit Log - UserID: {UserId}, Category: {Category}, Action: {Action}, Success: {Success}",
                    userId, category, action, actionSuccess);
            }
            catch (Exception ex)
            {
                Log.Error(ex, 
                    "Failed to create audit log for User {UserId}, Category: {Category}, Action: {Action}",
                    userId, category, action);
            }
        }

    }
}