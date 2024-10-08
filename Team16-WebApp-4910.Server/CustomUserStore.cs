// using Microsoft.AspNetCore.Identity;
// using System.Threading;
// using System.Threading.Tasks;
// using Team16_WebApp_4910.Server.Models;
// using System;
// using Microsoft.EntityFrameworkCore;

// namespace Team16_WebApp_4910.Server
// {
//     public class CustomUserStore : IUserStore<Users>, IUserPasswordStore<Users>, IUserEmailStore<Users>, IUserRoleStore<Users>
//     {
//         private readonly AppDBContext _context;

//         public CustomUserStore(AppDBContext context)
//         {
//             _context = context;
//         }

//         public async Task<IdentityResult> CreateAsync(Users user, CancellationToken cancellationToken)
//         {
//             _context.Users.Add(user);
//             await _context.SaveChangesAsync(cancellationToken);
//             return IdentityResult.Success;
//         }

//         public async Task<IdentityResult> DeleteAsync(Users user, CancellationToken cancellationToken)
//         {
//             _context.Users.Remove(user);
//             await _context.SaveChangesAsync(cancellationToken);
//             return IdentityResult.Success;
//         }

//         public async Task<Users> FindByIdAsync(string userId, CancellationToken cancellationToken)
//         {
//             return await _context.Users.FindAsync(new object[] { int.Parse(userId) }, cancellationToken);
//         }

//         public async Task<Users> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
//         {
//             return await _context.Users.FirstOrDefaultAsync(u => u.Username.ToUpper() == normalizedUserName, cancellationToken);
//         }

//         public Task<string> GetNormalizedUserNameAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(user.Username.ToUpper());
//         }

//         public Task<string> GetUserIdAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(user.UserID.ToString());
//         }

//         public Task<string> GetUserNameAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(user.Username);
//         }

//         public Task SetNormalizedUserNameAsync(Users user, string normalizedName, CancellationToken cancellationToken)
//         {
//             user.Username = normalizedName;
//             return Task.CompletedTask;
//         }

//         public Task SetUserNameAsync(Users user, string userName, CancellationToken cancellationToken)
//         {
//             user.Username = userName;
//             return Task.CompletedTask;
//         }

//         public async Task<IdentityResult> UpdateAsync(Users user, CancellationToken cancellationToken)
//         {
//             _context.Users.Update(user);
//             await _context.SaveChangesAsync(cancellationToken);
//             return IdentityResult.Success;
//         }

//         public Task SetPasswordHashAsync(Users user, string passwordHash, CancellationToken cancellationToken)
//         {
//             user.PasswordHash = passwordHash;
//             return Task.CompletedTask;
//         }

//         public Task<string> GetPasswordHashAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(user.PasswordHash);
//         }

//         public Task<bool> HasPasswordAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
//         }

//         public Task SetEmailAsync(Users user, string email, CancellationToken cancellationToken)
//         {
//             user.Email = email;
//             return Task.CompletedTask;
//         }

//         public Task<string> GetEmailAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(user.Email);
//         }

//         public Task<bool> GetEmailConfirmedAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(true); // Implement email confirmation if needed
//         }

//         public Task SetEmailConfirmedAsync(Users user, bool confirmed, CancellationToken cancellationToken)
//         {
//             return Task.CompletedTask; // Implement email confirmation if needed
//         }

//         public async Task<Users> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
//         {
//             return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToUpper() == normalizedEmail, cancellationToken);
//         }

//         public Task<string> GetNormalizedEmailAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(user.Email.ToUpper());
//         }

//         public Task SetNormalizedEmailAsync(Users user, string normalizedEmail, CancellationToken cancellationToken)
//         {
//             return Task.CompletedTask; // You might want to store normalized email if needed
//         }

//         public Task AddToRoleAsync(Users user, string roleName, CancellationToken cancellationToken)
//         {
//             user.Role = roleName;
//             return Task.CompletedTask;
//         }

//         public Task RemoveFromRoleAsync(Users user, string roleName, CancellationToken cancellationToken)
//         {
//             if (user.Role == roleName)
//                 user.Role = null;
//             return Task.CompletedTask;
//         }

//         public Task<IList<string>> GetRolesAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult((IList<string>)new List<string> { user.Role });
//         }

//         public Task<bool> IsInRoleAsync(Users user, string roleName, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(user.Role == roleName);
//         }

//         public Task<IList<Users>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
//         {
//             return Task.FromResult((IList<Users>)_context.Users.Where(u => u.Role == roleName).ToList());
//         }

//         public Task<DateTimeOffset?> GetLockoutEndDateAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult((DateTimeOffset?)null);
//         }

//         public Task SetLockoutEndDateAsync(Users user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
//         {
//             return Task.CompletedTask;
//         }

//         public Task<int> GetAccessFailedCountAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(0);
//         }

//         public Task<bool> GetLockoutEnabledAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.FromResult(false);
//         }

//         public Task SetLockoutEnabledAsync(Users user, bool enabled, CancellationToken cancellationToken)
//         {
//             return Task.CompletedTask;
//         }

//         public Task ResetAccessFailedCountAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.CompletedTask;
//         }

//         public Task IncrementAccessFailedCountAsync(Users user, CancellationToken cancellationToken)
//         {
//             return Task.CompletedTask;
//         }

//         public void Dispose()
//         {
//             // Dispose of any resources if needed
//         }
//     }
// }