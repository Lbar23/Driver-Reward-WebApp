using Microsoft.AspNetCore.Identity;
using Backend_Server.Models;

namespace Backend_Server.Infrastructure
{
    public class UserPasswordValidator : IPasswordValidator<Users>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<Users> manager, Users user, string? password)
        {
            var errors = new List<IdentityError>();
            
            if (await manager.IsInRoleAsync(user, "Admin"))
            {
                if (password?.Length < 12)
                {
                    errors.Add(new IdentityError
                    {
                        Code = "AdminPasswordLength",
                        Description = "Admin passwords must be at least 12 characters long."
                    });
                }

                if (password != null && !password.Any(char.IsSymbol) && !password.Any(char.IsPunctuation))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "AdminPasswordSymbol",
                        Description = "Admin passwords must contain at least three symbols."
                    });
                }
            }
            else if (await manager.IsInRoleAsync(user, "Driver"))
            {
                if (password?.Length < 8)
                {
                    errors.Add(new IdentityError
                    {
                        Code = "DriverPasswordLength",
                        Description = "Driver passwords must be at least 8 characters long."
                    });
                }

                if (password != null && !password.Any(char.IsUpper))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "DriverPasswordUpper",
                        Description = "Password must contain at least one uppercase letter."
                    });
                }

                if (password != null && !password.Any(c => char.IsSymbol(c) || char.IsPunctuation(c)))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "DriverPasswordSymbol",
                        Description = "Password must contain at least one special character."
                    });
                }
            }
            else
            {
                if (password?.Length < 6)
                {
                    errors.Add(new IdentityError
                    {
                        Code = "GuestPasswordLength",
                        Description = "Password must be at least 6 characters long."
                    });
                }

                if (password != null && !password.Any(char.IsDigit))
                {
                    errors.Add(new IdentityError
                    {
                        Code = "GuestPasswordNumber",
                        Description = "Password must contain at least one number."
                    });
                }
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. errors]);
        }
    }
}