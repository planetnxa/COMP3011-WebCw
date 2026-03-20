using Microsoft.AspNetCore.Mvc;
using WebAppComp3011.Models;
using System.Text.Json;

namespace WebAppComp3011.Controllers
{
    public class FragrancesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FragrancesController> _logger;

        public FragrancesController(IHttpClientFactory httpClientFactory, ILogger<FragrancesController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync("api/fragrances");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var fragrances = JsonSerializer.Deserialize<List<Fragrance>>(content, options);

                    return View(fragrances ?? new List<Fragrance>());
                }
                else
                {
                    _logger.LogError($"API returned status: {response.StatusCode}");
                    return View(new List<Fragrance>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching fragrances: {ex.Message}");
                return View(new List<Fragrance>());
            }
        }

        public IActionResult Edit()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }
    }
}
