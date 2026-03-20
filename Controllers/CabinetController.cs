using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebAppComp3011.Models;

namespace WebAppComp3011.Controllers
{
    public class CabinetController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CabinetController> _logger;

        public CabinetController(IHttpClientFactory httpClientFactory, ILogger<CabinetController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Get all cabinet entries for this user
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

            // For each cabinet entry, fetch the full fragrance details
            var cabinetItems = new List<(UserCabinet Entry, Fragrance Fragrance)>();
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
                            cabinetItems.Add((entry, frag));
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

            return View(cabinetItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cabinetId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

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

            return RedirectToAction("Index");
        }
    }
}
