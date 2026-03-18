using System;
using System.Collections.Generic;

namespace WebAppComp3011.Models
{
    public class Fragrance
    {
        // Map to perf100 table columns
        public int Id { get; set; }
        public string FragUrl { get; set; } // url
        public string FragName { get; set; } // clean
        public string Brand { get; set; }
        public string Country { get; set; }
        public string Gender { get; set; }
        public float Rating { get; set; } // Rating column (REAL)
        public string Year { get; set; }

        public Notes Notes { get; set; } = new Notes();

        // Accords and Perfumers are stored as TEXT (likely comma-separated) in perf100 table
        public List<string> Accords { get; set; }
        public string Perfumers { get; set; }
    }

    public class Notes
    {
        // store as comma-separated strings for binding in views
        public List<string> Top { get; set; }
        public List<string> Middle { get; set; }
        public List<string> Base { get; set; }
    }
}
