using Microsoft.AspNetCore.Mvc;
using WebAppComp3011.Models;
using System.Text;
using System.Text.Json;

namespace WebAppComp3011.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IHttpClientFactory httpClientFactory, ILogger<LoginController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: Login/Index - Display login form
        public IActionResult Index()
        {
            return View("Login");
        }

        // POST: Login/Login - Handle login attempt
        [HttpPost("Login/Login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Username and password are required.");
                return View("Login");
            }

            try
            {
                // Get HttpClient instance from factory
                var httpClient = _httpClientFactory.CreateClient("ApiClient");

                // Call API to get user by username
                var response = await httpClient.GetAsync($"api/userprofile/username/{username}");

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var user = JsonSerializer.Deserialize<UserProfile>(content, options);

                    // Validate password
                    if (user != null && user.Password == password)
                    {
                        // Login successful - set session
                        HttpContext.Session.SetString("UserId", user.Id.ToString());
                        HttpContext.Session.SetString("Username", user.Username);
                        _logger.LogInformation($"User {username} logged in successfully.");

                        return RedirectToPage("/Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid password.");
                        return View("Login");
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ModelState.AddModelError("", "Username not found.");
                    return View("Login");
                }
                else
                {
                    _logger.LogError($"API returned status: {response.StatusCode}");
                    ModelState.AddModelError("", "An error occurred during login. Please try again.");
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                ModelState.AddModelError("", "Unable to connect to authentication service.");
                return View("Login");
            }
        }

        // GET: Login/Register - Display registration form
        [HttpGet("Login/Register")]
        public IActionResult Register()
        {
            return View("Register");
        }

        // POST: Login/Register - Handle account creation
        [HttpPost("Login/Register")]
        public async Task<IActionResult> Register(string username, string password, string confirmPassword, string name)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View("Register");
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View("Register");
            }

            if (password.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters.");
                return View("Register");
            }

            try
            {
                // Get HttpClient instance from factory
                var httpClient = _httpClientFactory.CreateClient("ApiClient");

                // Check if username already exists
                var checkResponse = await httpClient.GetAsync($"api/userprofile/username/{username}");

                if (checkResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ModelState.AddModelError("", "Username already exists. Please choose another.");
                    return View("Register");
                }

                // Create new user account via API
                var newProfile = new UserProfile
                {
                    Username = username,
                    Password = password,
                    Name = name,
                    FirstLogin = true
                };

                var json = JsonSerializer.Serialize(newProfile);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var createResponse = await httpClient.PostAsync("api/userprofile", content);

                if (createResponse.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    var responseContent = await createResponse.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var createdUser = JsonSerializer.Deserialize<UserProfile>(responseContent, options);

                    // Registration successful - auto-login the user
                    if (createdUser != null)
                    {
                        HttpContext.Session.SetString("UserId", createdUser.Id.ToString());
                        HttpContext.Session.SetString("Username", createdUser.Username);
                        _logger.LogInformation($"New user {username} registered and logged in.");

                        return RedirectToPage("/Index");
                    }
                }
                else if (createResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    ModelState.AddModelError("", "Invalid registration data. Please check your inputs.");
                    return View("Register");
                }
                else
                {
                    _logger.LogError($"API returned status: {createResponse.StatusCode}");
                    ModelState.AddModelError("", "Failed to create account. Please try again.");
                    return View("Register");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                ModelState.AddModelError("", "Unable to connect to authentication service.");
                return View("Register");
            }

            return View("Register");
        }

        // GET: Login/Logout - Handle logout
        [HttpGet("Login/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            _logger.LogInformation("User logged out.");
            return RedirectToPage("/Index");
        }
    }
}
