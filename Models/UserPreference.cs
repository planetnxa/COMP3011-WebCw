using System;

namespace WebAppComp3011.Models
{
    public class UserPreference
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PrefVal { get; set; } = string.Empty;
        public string PrefType { get; set; } = string.Empty;
    }
}
