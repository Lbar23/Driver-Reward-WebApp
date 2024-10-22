using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum NotificationPref
    {
        None,   // In case 2FA is not enabled
        Phone,  
        Email  
    }
    public class Users : IdentityUser<int>
    {
        public required string UserType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public NotificationPref NotifyPref { get; set; } = NotificationPref.None; 
    }

}