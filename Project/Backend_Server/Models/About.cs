using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend_Server.Models
{
    public class About
    {
        public DateTime Release { get; set; }
        public int Version { get; set; }
        public int Team { get; set; } = 16;
        public string Product { get; set; } = "GitGud Drivers";
        public string Description { get; set; }= @"Our Program, GitGud Drivers, aims to transform the trucking industry " +
        "with our innovative web application designed to incentivize safe and efficent driving." + 
        "Our platform allows companies to reward truck drivers for positive drivers, " +
        "offering points that can be redeemed for a variety of products produced by the sponsor company.";
    }
}