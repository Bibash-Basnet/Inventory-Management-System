
using Inventory.Models.DTO.Auth;
using Inventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IApiClient apiClient, ILogger<AuthController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to products
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            {
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill in all required fields.";
                return View(dto);
            }

            try
            {
                // ✅ FIXED: API doesn't need role in login request
                var loginRequest = new
                {
                    username = dto.Username,
                    password = dto.Password
                };

                var response = await _apiClient.PostAsync<AuthResponseDto>("Auth/login", loginRequest);

                if (response == null || string.IsNullOrEmpty(response.Token))
                {
                    ViewBag.Error = "Invalid username or password.";
                    return View(dto);
                }

                // Store token, username, and role in session
                HttpContext.Session.SetString("JWT", response.Token);
                HttpContext.Session.SetString("Username", response.Username);
                HttpContext.Session.SetString("Role", response.Role);

                _logger.LogInformation($"User logged in: {response.Username}, Role: {response.Role}");

                TempData["Success"] = $"Welcome back, {response.Username}!";
                return RedirectToAction("Index", "Product");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Login error: {ex.Message}");

                if (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
                {
                    ViewBag.Error = "Invalid username or password.";
                }
                else
                {
                    ViewBag.Error = "Unable to connect to server. Please try again later.";
                }
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                ViewBag.Error = "An error occurred. Please try again.";
                return View(dto);
            }
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            // If already logged in, redirect to products
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JWT")))
            {
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Please fill in all required fields correctly.";
                return View(dto);
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View(dto);
            }

            try
            {
                var response = await _apiClient.PostAsync<AuthResponseDto>("Auth/register", dto);

                if (response == null || string.IsNullOrEmpty(response.Token))
                {
                    ViewBag.Error = "Registration failed. Try another username or email.";
                    return View(dto);
                }

                _logger.LogInformation($"User registered: {dto.Username}, Role: {dto.Role}");

                TempData["Success"] = $"Account created successfully! Please login.";
                return RedirectToAction("Login");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");

                if (ex.Message.Contains("400") || ex.Message.Contains("BadRequest"))
                {
                    ViewBag.Error = "Username or email already exists.";
                }
                else
                {
                    ViewBag.Error = "Unable to connect to server. Please try again later.";
                }
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                ViewBag.Error = "Registration failed. Please try again.";
                return View(dto);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username");
            _logger.LogInformation($"User logged out: {username}");

            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}