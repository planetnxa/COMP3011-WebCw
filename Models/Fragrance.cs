using System;
using System.Collections.Generic;

namespace WebAppComp3011.Models
{
    public class Fragrance
    {
        // Map to perf100 table columns
        public int Id { get; set; }
        public string FragUrl { get; set; } = string.Empty; // url
        public string FragName { get; set; } = string.Empty; // clean
        public string Brand { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public float Rating { get; set; } // Rating column (REAL)
        public string Year { get; set; } = string.Empty;

        public Notes Notes { get; set; } = new Notes();

        // Accords and Perfumers are stored as TEXT (likely comma-separated) in perf100 table
        public List<string> Accords { get; set; } = new();
        public string Perfumers { get; set; } = string.Empty;
    }

    public class Notes
    {
        // store as comma-separated strings for binding in views
        public List<string> Top { get; set; } = new();
        public List<string> Middle { get; set; } = new();
        public List<string> Base { get; set; } = new();
    }
}
