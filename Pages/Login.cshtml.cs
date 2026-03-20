using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppComp3011.Models;
using WebAppComp3011.Services;
using System.Text.Json;

namespace WebAppComp3011.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync($"/api/userProfile/username/{Username}");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var user = JsonSerializer.Deserialize<UserProfile>(content, options);

                    if (user != null && PasswordHashingService.VerifyPassword(Password, user.Password))
                    {
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("Username", user.Username);
                        _logger.LogInformation($"User {Username} logged in successfully.");

                        if (user.FirstLogin)
                        {
                            _logger.LogInformation($"User {Username} is on first login - redirecting to Setup.");
                            return RedirectToPage("/Setup");
                        }

                        return RedirectToPage("/Cabinet");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid password.");
                        return Page();
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError("", "Username not found.");
                    return Page();
                }
                else
                {
                    _logger.LogError($"API returned status: {response.StatusCode}");
                    ModelState.AddModelError("", "An error occurred during login. Please try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                ModelState.AddModelError("", "Unable to connect to authentication service.");
                return Page();
            }
        }
    }
}
