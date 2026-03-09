using Microsoft.EntityFrameworkCore;

namespace WebAppComp3011.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) 
            : base(options)
        {

        }
        public DbSet<TodoItem> TodoItems { get; set; } = null;
    }
}
