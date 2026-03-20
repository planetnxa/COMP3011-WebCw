using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Text.Json;
using WebAppComp3011.Models;

namespace WebAppComp3011.Pages
{
    public class SetupModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SetupModel> _logger;

        public SetupModel(IHttpClientFactory httpClientFactory, ILogger<SetupModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public string SearchQuery { get; set; }

        [BindProperty]
        public string SearchType { get; set; } = "name"; // name, brand, accord, notes

        public List<Fragrance> SearchResults { get; set; } = new();
        public List<Fragrance> SelectedFragrances { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                ModelState.AddModelError("", "Please enter a search query.");
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                string endpoint = SearchType switch
                {
                    "brand" => $"/api/frag/brand/{Uri.EscapeDataString(SearchQuery)}",
                    "accord" => $"/api/frag/accord/{Uri.EscapeDataString(SearchQuery)}",
                    "notes" => $"/api/frag/notes/{Uri.EscapeDataString(SearchQuery)}",
                    _ => $"/api/frag/name/{Uri.EscapeDataString(SearchQuery)}"
                };

                var response = await httpClient.GetAsync(endpoint);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    SearchResults = JsonSerializer.Deserialize<List<Fragrance>>(content, options) ?? new();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    SearchResults = new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Search error: {ex.Message}");
                ModelState.AddModelError("", "Error searching fragrances. Please try again.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddToLikeAsync(int fragranceId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Index");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");

                var cabinet = new UserCabinet
                {
                    UserId = int.Parse(userId),
                    Username = username,
                    PerfumeId = fragranceId,
                    Comments = ""
                };

                var json = JsonSerializer.Serialize(cabinet);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("/api/usercabinet", content);

                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                {
                    _logger.LogError($"Failed to add fragrance to cabinet: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Add to cabinet error: {ex.Message}");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCompleteSetupAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Index");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");

                // Update user profile to set FirstLogin to false
                var updatePayload = new { firstLogin = false };
                var json = JsonSerializer.Serialize(updatePayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync($"/api/userprofile/{userId}", content);

                if (response.StatusCode == System.Net.HttpStatusCode.OK || 
                    response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation($"User {userId} setup completed, FirstLogin set to false.");
                    return RedirectToPage("/Fragrances");
                }
                else
                {
                    _logger.LogError($"Failed to update user profile: {response.StatusCode}");
                    ModelState.AddModelError("", "Error completing setup. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Complete setup error: {ex.Message}");
                ModelState.AddModelError("", "Error completing setup. Please try again.");
            }

            return Page();
        }
    }
}
