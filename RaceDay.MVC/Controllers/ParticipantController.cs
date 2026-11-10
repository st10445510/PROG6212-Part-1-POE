using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient GetApiClient()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        public async Task<IActionResult> Categories()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            var client = GetApiClient();
            var response = await client.GetAsync("api/Categories");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load categories.";
                return View(new List<CategoryViewModel>());
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<CategoryViewModel>>();

            return View(categories ?? new List<CategoryViewModel>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            var client = GetApiClient();

            var response = await client.PostAsJsonAsync(
                "api/Enrolments",
                new { CategoryId = categoryId });

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "You have successfully enrolled!";
            }
            else
            {
                TempData["Error"] = await response.Content.ReadAsStringAsync();
            }

            return RedirectToAction("Categories");
        }

        public async Task<IActionResult> MyEnrolments()
        {
            if (HttpContext.Session.GetString("Role") != "Participant")
                return RedirectToAction("Login", "Auth");

            var client = GetApiClient();
            var response = await client.GetAsync("api/Enrolments");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load your enrolments.";
                return View(new List<System.Text.Json.JsonElement>());
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<System.Text.Json.JsonElement>>();

            return View(enrolments ?? new List<System.Text.Json.JsonElement>());
        }
    }
}