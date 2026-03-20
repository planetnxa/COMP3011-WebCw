using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppComp3011.Models;
using System.Text;
using System.Text.Json;

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

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            _logger.LogInformation("Setup page loaded.");
            return Page();
        }

        public async Task<IActionResult> OnPostSearchAsync([FromBody] SearchRequest request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (request == null || string.IsNullOrWhiteSpace(request.SearchQuery))
                return BadRequest("Search query is required.");

            _logger.LogInformation($"Search called with query='{request.SearchQuery}', type='{request.SearchType}'");

            var searchResults = new List<Fragrance>();
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                string endpoint = request.SearchType switch
                {
                    "brand" => $"api/frag/brand/{Uri.EscapeDataString(request.SearchQuery)}",
                    "accord" => $"api/frag/accord/{Uri.EscapeDataString(request.SearchQuery)}",
                    _ => $"api/frag/name/{Uri.EscapeDataString(request.SearchQuery)}"
                };
                _logger.LogInformation($"Calling API endpoint: {httpClient.BaseAddress}{endpoint}");
                var response = await httpClient.GetAsync(endpoint);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    searchResults = JsonSerializer.Deserialize<List<Fragrance>>(content, options) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Search error: {ex.Message}");
                return StatusCode(500, "An error occurred during search.");
            }

            return new JsonResult(searchResults);
        }

        public async Task<IActionResult> OnPostAddPreferenceAsync(string prefVal, string prefType)
        {
            _logger.LogInformation($"AddPreference called with prefVal='{prefVal}', prefType='{prefType}'");
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var pref = new UserPreference
                {
                    UserId = int.Parse(userId),
                    PrefVal = prefVal,
                    PrefType = prefType
                };
                var json = JsonSerializer.Serialize(pref);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/userpreference", content);
                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                    _logger.LogError($"Failed to add preference: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Add preference error: {ex.Message}");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteSetupAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            await SetFirstLoginFalseAsync(userId);
            return RedirectToPage("/Fragrances");
        }

        public async Task<IActionResult> OnPostSkipSetupAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            await SetFirstLoginFalseAsync(userId);
            return RedirectToPage("/Fragrances");
        }

        private async Task SetFirstLoginFalseAsync(string userId)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Fetch the current profile so we can send the full object back
                var getResponse = await httpClient.GetAsync($"api/userprofile/{userId}");
                if (!getResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch user profile {userId}: {getResponse.StatusCode}");
                    return;
                }

                var profileJson = await getResponse.Content.ReadAsStringAsync();
                var profile = JsonSerializer.Deserialize<UserProfile>(profileJson, options);
                if (profile == null)
                {
                    _logger.LogError($"Failed to deserialize user profile {userId}");
                    return;
                }

                // Update firstLogin and PUT the full profile back
                profile.FirstLogin = false;
                var json = JsonSerializer.Serialize(profile);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var putResponse = await httpClient.PutAsync($"api/userprofile/{userId}", content);

                if (putResponse.StatusCode == System.Net.HttpStatusCode.OK || putResponse.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation($"User {userId} setup completed, FirstLogin set to false.");
                }
                else
                {
                    var body = await putResponse.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to update user profile {userId}: {putResponse.StatusCode} - {body}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"SetFirstLoginFalse error for user {userId}: {ex.Message}");
            }
        }
    }
}
