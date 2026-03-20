using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppComp3011.Models;
using System.Text;
using System.Text.Json;

namespace WebAppComp3011.Pages
{
    public class RecommendModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RecommendModel> _logger;

        public RecommendModel(IHttpClientFactory httpClientFactory, ILogger<RecommendModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public List<Fragrance> Recommendations { get; set; } = new();
        public List<UserPreference> UserPreferences { get; set; } = new();

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
                    UserPreferences = JsonSerializer.Deserialize<List<UserPreference>>(prefContent, options) ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user preferences: {ex.Message}");
            }

            if (UserPreferences.Count > 0)
            {
                // Use preferences to find matching fragrances, then randomly pick 3
                var allMatches = new List<Fragrance>();
                var random = new Random();

                // Shuffle preferences and try each until we have enough candidates
                var shuffledPrefs = UserPreferences.OrderBy(_ => random.Next()).ToList();
                foreach (var pref in shuffledPrefs)
                {
                    try
                    {
                        string endpoint = pref.PrefType switch
                        {
                            "brand" => $"api/frag/brand/{Uri.EscapeDataString(pref.PrefVal)}",
                            "accord" => $"api/frag/accord/{Uri.EscapeDataString(pref.PrefVal)}",
                            _ => $"api/frag/name/{Uri.EscapeDataString(pref.PrefVal)}"
                        };

                        var response = await httpClient.GetAsync(endpoint);
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var results = JsonSerializer.Deserialize<List<Fragrance>>(content, options) ?? new();
                            foreach (var frag in results)
                            {
                                if (allMatches.All(m => m.Id != frag.Id))
                                    allMatches.Add(frag);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error fetching fragrances for pref '{pref.PrefVal}': {ex.Message}");
                    }
                }

                // Randomly pick 3 from all matches
                if (allMatches.Count > 0)
                {
                    Recommendations = allMatches.OrderBy(_ => random.Next()).Take(3).ToList();
                }
            }
            else
            {
                // No preferences set — fall back to random fragrances
                var random = new Random();
                int attempts = 0;
                while (Recommendations.Count < 3 && attempts < 15)
                {
                    attempts++;
                    int randomId = random.Next(1, 24001);
                    try
                    {
                        var response = await httpClient.GetAsync($"api/frag/{randomId}");
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var frag = JsonSerializer.Deserialize<Fragrance>(content, options);
                            if (frag != null && Recommendations.All(r => r.Id != frag.Id))
                            {
                                Recommendations.Add(frag);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error fetching random fragrance {randomId}: {ex.Message}");
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCabinetAsync(int perfumeId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var cabinet = new UserCabinet
                {
                    UserId = int.Parse(userId),
                    Username = username ?? string.Empty,
                    PerfumeId = perfumeId,
                    Comments = ""
                };
                var json = JsonSerializer.Serialize(cabinet);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/usercabinet", content);
                if (!response.IsSuccessStatusCode)
                    _logger.LogError($"Failed to add fragrance to cabinet: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Add to cabinet error: {ex.Message}");
            }

            return new OkResult();
        }
    }
}
