using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppComp3011.Models;
using System.Text;
using System.Text.Json;

namespace WebAppComp3011.Pages
{
    public class CabinetModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CabinetModel> _logger;

        public CabinetModel(IHttpClientFactory httpClientFactory, ILogger<CabinetModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public List<(UserCabinet Entry, Fragrance Fragrance)> CabinetItems { get; set; } = new();
        public List<UserPreference> Preferences { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Fetch user preferences
            try
            {
                var prefResponse = await httpClient.GetAsync($"api/userpreference/user/{userId}");
                if (prefResponse.IsSuccessStatusCode)
                {
                    var prefContent = await prefResponse.Content.ReadAsStringAsync();
                    Preferences = JsonSerializer.Deserialize<List<UserPreference>>(prefContent, options) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching preferences: {ex.Message}");
            }

            // Fetch cabinet entries
            var cabinetEntries = new List<UserCabinet>();
            try
            {
                var response = await httpClient.GetAsync($"api/usercabinet/user/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    cabinetEntries = JsonSerializer.Deserialize<List<UserCabinet>>(content, options) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching cabinet: {ex.Message}");
            }

            foreach (var entry in cabinetEntries)
            {
                if (entry.PerfumeId == null) continue;
                try
                {
                    var fragResponse = await httpClient.GetAsync($"api/frag/{entry.PerfumeId}");
                    if (fragResponse.IsSuccessStatusCode)
                    {
                        var fragContent = await fragResponse.Content.ReadAsStringAsync();
                        var frag = JsonSerializer.Deserialize<Fragrance>(fragContent, options);
                        if (frag != null)
                            CabinetItems.Add((entry, frag));
                    }
                    else
                    {
                        _logger.LogWarning($"Fragrance {entry.PerfumeId} not found for cabinet entry {entry.Id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching fragrance {entry.PerfumeId}: {ex.Message}");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int cabinetId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.DeleteAsync($"api/usercabinet/{cabinetId}");
                if (!response.IsSuccessStatusCode)
                    _logger.LogError($"Failed to remove cabinet entry {cabinetId}: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Remove cabinet entry error: {ex.Message}");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditPreferenceAsync(int prefId, string prefVal, string prefType)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var pref = new UserPreference
                {
                    Id = prefId,
                    UserId = int.Parse(userId),
                    PrefVal = prefVal,
                    PrefType = prefType
                };
                var json = JsonSerializer.Serialize(pref);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"api/userpreference/{prefId}", content);
                if (!response.IsSuccessStatusCode)
                    _logger.LogError($"Failed to update preference {prefId}: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Edit preference error: {ex.Message}");
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePreferenceAsync(int prefId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.DeleteAsync($"api/userpreference/{prefId}");
                if (!response.IsSuccessStatusCode)
                    _logger.LogError($"Failed to delete preference {prefId}: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete preference error: {ex.Message}");
            }

            return RedirectToPage();
        }
    }
}
