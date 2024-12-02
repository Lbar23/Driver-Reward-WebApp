using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{

    public class NotificationHistory
    {
        public required int UserID { get; set; }
        public int InstanceID { get; set; }  // log id for the individual notfication 
        public int? NotifyTypeID { get; set; }  // for the notification type
        
        public bool Success { get; set; }  = false;   // failed notification is 0
        public required DateTime NotifyDate { get; set; }
        [ForeignKey(nameof(NotifyTypeID))]
        public virtual NotifyTypes? NotifyTypes { get; set; } // Navigation property
        [ForeignKey(nameof(UserID))]
        public virtual Users? User { get; set; } // Navigation property
    }
}