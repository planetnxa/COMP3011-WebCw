using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAppComp3011.Models;

namespace WebAppComp3011.Data
{
    public class FragranceContext : DbContext
    {
        public FragranceContext (DbContextOptions<FragranceContext> options)
            : base(options)
        {
        }

        public DbSet<WebAppComp3011.Models.Fragrance> Fragrance { get; set; } = default!;
    }
}
