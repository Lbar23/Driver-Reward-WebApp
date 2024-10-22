using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class Admins
    {
        public int AdminID { get; set; }
        public int UserID { get; set; }
        public required Users User { get; set; }
    }
}