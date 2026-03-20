using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebAppComp3011.Pages
{
    public class IndexModel : PageModel
    {
        public bool IsLoggedIn { get; set; }
        public string Username { get; set; }

        public void OnGet()
        {
            Username = HttpContext.Session.GetString("Username");
            IsLoggedIn = !string.IsNullOrEmpty(Username);
        }
    }
}
