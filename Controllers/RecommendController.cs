using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using WebAppComp3011.Models;

namespace WebAppComp3011.Controllers
{
    public class RecommendController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RecommendController> _logger;

        public RecommendController(IHttpClientFactory httpClientFactory, ILogger<RecommendController> logger)
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
            var random = new Random();
            var recommendations = new List<Fragrance>();

            // Try to fetch 3 random perfumes; retry with new IDs if one doesn't exist
            int attempts = 0;
            while (recommendations.Count < 3 && attempts < 15)
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
                        if (frag != null && recommendations.All(r => r.Id != frag.Id))
                        {
                            recommendations.Add(frag);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error fetching random fragrance {randomId}: {ex.Message}");
                }
            }

            return View(recommendations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCabinet(int perfumeId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var cabinet = new UserCabinet
                {
                    UserId = int.Parse(userId),
                    Username = username,
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

            return RedirectToAction("Index");
        }
    }
}
