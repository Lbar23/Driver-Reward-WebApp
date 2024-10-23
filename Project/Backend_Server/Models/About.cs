using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class About
    {
        public Date Release { get; set; }
        public int Version { get; set; }
        public int Team { get; set; }
        public string Product { get; set; }
        public string Description { get; set; }
    }
}