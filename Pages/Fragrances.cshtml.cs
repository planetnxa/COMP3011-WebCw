using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAppComp3011.Models;
using System.Text;
using System.Text.Json;

namespace WebAppComp3011.Pages
{
    public class FragrancesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FragrancesModel> _logger;

        public FragrancesModel(IHttpClientFactory httpClientFactory, ILogger<FragrancesModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Index");

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

        public async Task<IActionResult> OnPostAddToCabinetAsync(int fragranceId)
        {
            _logger.LogInformation($"AddToCabinet called with fragranceId={fragranceId}");
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

            return RedirectToPage();
        }
    }
}
