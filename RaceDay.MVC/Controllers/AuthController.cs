
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Registration successful. Please log in.";
                return RedirectToAction("Login");
            }

            var errorText = await response.Content.ReadAsStringAsync();

            try
            {
                using var document = JsonDocument.Parse(errorText);

                ViewBag.Error = document.RootElement.TryGetProperty("message", out var message)
                    ? message.GetString()
                    : "Registration failed. Please try again.";
            }
            catch (JsonException)
            {
                ViewBag.Error = string.IsNullOrWhiteSpace(errorText)
                    ? "Registration failed. Please try again."
                    : errorText;
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();

            HttpContext.Session.SetString(
                "JwtToken", result.GetProperty("token").GetString()!);

            HttpContext.Session.SetString(
                "UserId", result.GetProperty("userId").ToString());

            HttpContext.Session.SetString(
                "FirstName", result.GetProperty("firstName").GetString()!);

            HttpContext.Session.SetString(
                "LastName", result.GetProperty("lastName").GetString()!);

            HttpContext.Session.SetString(
                "Email", result.GetProperty("email").GetString()!);

            HttpContext.Session.SetString(
                "Role", result.GetProperty("role").GetString()!);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
