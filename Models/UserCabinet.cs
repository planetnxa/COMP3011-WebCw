using System;

namespace WebAppComp3011.Models
{
    public class UserCabinet
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public int? PerfumeId { get; set; }
        public int? UserId { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}
