using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public enum UserPermissions
    {
        ReportAdmin,
        ReportSponsor,
        ManageAdmin, 
        ManageDriver,
        ManageSponsor,
        Purchase,
        ManagePoint, 
        System  // catch all admin permissions
    }
    public class Permissions
    {
        public int PermissionID { get; set; }
        public required string Role { get; set; }  // fk from aspnet roles
        public required UserPermissions Permission { get; set; }
    }
}