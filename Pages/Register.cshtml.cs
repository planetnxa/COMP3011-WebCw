using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppComp3011.Models;
using System.Text;
using System.Text.Json;

namespace WebAppComp3011.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IHttpClientFactory httpClientFactory, ILogger<RegisterModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public string Name { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Name))
            {
                ModelState.AddModelError("", "All fields are required.");
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return Page();
            }

            if (Password.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters.");
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var checkResponse = await httpClient.GetAsync($"/api/userProfile/username/{Username}");

                if (checkResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ModelState.AddModelError("", "Username already exists. Please choose another.");
                    return Page();
                }

                var newProfile = new UserProfile
                {
                    Username = Username,
                    Password = Password,
                    Name = Name,
                    FirstLogin = true
                };

                var json = JsonSerializer.Serialize(newProfile);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var createResponse = await httpClient.PostAsync("/api/userProfile", content);

                if (createResponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var responseContent = await createResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var createdUser = JsonSerializer.Deserialize<UserProfile>(responseContent, options);

                    if (createdUser != null)
                    {
                        HttpContext.Session.SetString("UserId", createdUser.Id.ToString());
                        HttpContext.Session.SetString("Username", createdUser.Username);
                        _logger.LogInformation($"New user {Username} registered and logged in.");
                        return RedirectToPage("/Setup");
                    }
                }
                else if (createResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    ModelState.AddModelError("", "Invalid registration data. Please check your inputs.");
                    return Page();
                }
                else
                {
                    _logger.LogError($"API returned status: {createResponse.StatusCode}");
                    ModelState.AddModelError("", "Failed to create account. Please try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                ModelState.AddModelError("", "Unable to connect to authentication service.");
                return Page();
            }

            return Page();
        }
    }
}
