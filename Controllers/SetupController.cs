using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using WebAppComp3011.Models;
using Microsoft.Extensions.Logging;

namespace WebAppComp3011.Controllers
{
    public class SetupController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SetupController> _logger;

        public SetupController(IHttpClientFactory httpClientFactory, ILogger<SetupController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");

            ViewBag.SearchResults = new List<Fragrance>();
            ViewBag.SearchQuery = "";
            ViewBag.SearchType = "name";
            _logger.LogInformation("Setup page loaded.");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)
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
                    "notes" => $"api/frag/notes/{Uri.EscapeDataString(request.SearchQuery)}",
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

            return Ok(searchResults);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToLike(int fragranceId, string searchQuery, string searchType)
        {
            _logger.LogInformation($"AddToLike called with fragranceId={fragranceId}, searchQuery='{searchQuery}', searchType='{searchType}'");
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
                    PerfumeId = fragranceId,
                    Comments = ""
                };
                var json = JsonSerializer.Serialize(cabinet);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("api/usercabinet", content);
                if (response.StatusCode != System.Net.HttpStatusCode.Created)
                    _logger.LogError($"Failed to add fragrance to cabinet: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Add to cabinet error: {ex.Message}");
            }
            // Redirect back to setup - search results will still be visible on refresh or user can search again
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteSetup()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Index", "Home");
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var updatePayload = new { firstLogin = false };
                var json = JsonSerializer.Serialize(updatePayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync($"api/userprofile/{userId}", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.NoContent)
                {
                    _logger.LogInformation($"User {userId} setup completed, FirstLogin set to false.");
                    return RedirectToAction("Index", "Fragrances");
                }
                else
                {
                    _logger.LogError($"Failed to update user profile: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Complete setup error: {ex.Message}");
            }
            return View("Index");
        }
    }
}
