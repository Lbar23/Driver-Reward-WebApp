using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Permissions
    {
        public int PermissionID { get; set; }
        public required string Role { get; set; }
        public required string PermissionName { get; set; }

    }
}