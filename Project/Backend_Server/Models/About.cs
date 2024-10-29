using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class About
    {
        public DateOnly Release { get; set; }
        public int Team { get; set; }
        public int Version { get; set; }
        public required string Product { get; set; }
        public required string Description { get; set; }
    }
}