using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Team16_WebApp_4910.Server.Models
{
    public class Admins
    {
        public int AdminID { get; set; }
        public int UserID { get; set; }
        public Users User { get; set; }
    }
}