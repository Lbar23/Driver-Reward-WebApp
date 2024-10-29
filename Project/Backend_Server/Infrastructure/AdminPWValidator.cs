using Microsoft.AspNetCore.Identity;
using Backend_Server.Models;

namespace Backend_Server.Infrastructure
{
    public class AdminPasswordValidator : IPasswordValidator<Users>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<Users> manager, Users user, string? password)
        {
            var errors = new List<IdentityError>();
            
            //Check if user is admin
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

                if (password != null && !password.Any(char.IsSymbol) && !password.Any(char.IsPunctuation)) //null check
                {
                    errors.Add(new IdentityError
                    {
                        Code = "AdminPasswordSymbol",
                        Description = "Admin passwords must contain at least three symbols."
                    });
                }
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. errors]);
        }
    }
}