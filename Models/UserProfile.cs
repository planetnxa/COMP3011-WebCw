using System.Collections.Generic;

namespace WebAppComp3011.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        // stored as INTEGER in SQLite (0/1) - expose as bool for convenience
        public bool FirstLogin { get; set; }

        // optional navigation property - a user's cabinet entries
        public List<UserCabinet> Cabinet { get; set; } = new List<UserCabinet>();
    }
}
