//Simple Identity implementation

using Microsoft.AspNetCore.Identity;

namespace Backend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool Is2FAEnabled { get; set; }
        public bool Is2FASetupLater { get; set; }
    }
}