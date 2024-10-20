using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Permissions
    {
        public int PermissionID { get; set; }
        public string Role { get; set; }
        public string PermissionName { get; set; }

    }
}