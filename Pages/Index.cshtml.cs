using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppComp3011.Pages
{
    public class IndexModel : PageModel
    {
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public void OnGet()
        {
            Username = HttpContext.Session.GetString("Username") ?? string.Empty;
            DisplayName = HttpContext.Session.GetString("Name") ?? Username;
            IsLoggedIn = !string.IsNullOrEmpty(Username);
        }
    }
}
