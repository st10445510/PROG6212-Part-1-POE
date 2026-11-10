
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public OrganiserController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Enrolments()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var client = GetApiClient();
            var response = await client.GetAsync("api/Enrolments");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load enrolments.";
                return View(new List<JsonElement>());
            }

            var enrolments =
                await response.Content.ReadFromJsonAsync<List<JsonElement>>();

            return View(enrolments ?? new List<JsonElement>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var client = GetApiClient();

            var response = await client.PutAsJsonAsync(
                $"api/Enrolments/{id}/status",
                new { Status = status });

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Enrolment status updated successfully.";
            else
                TempData["Error"] = await response.Content.ReadAsStringAsync();

            return RedirectToAction("Enrolments");
        }

        public async Task<IActionResult> Results()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var client = GetApiClient();
            var response = await client.GetAsync("api/Results");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load results.";
                return View(new List<JsonElement>());
            }

            var results = await response.Content
                .ReadFromJsonAsync<List<JsonElement>>();

            return View(results ?? new List<JsonElement>());
        }

        [HttpGet]
        public IActionResult CreateResult()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResult(
            RaceDay.MVC.ViewModels.ResultViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
                return View(model);

            var client = GetApiClient();

            var response = await client.PostAsJsonAsync(
                "api/Results",
                model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Race result published successfully.";
                return RedirectToAction("Results");
            }

            ViewBag.Error = await response.Content.ReadAsStringAsync();
            return View(model);
        }

    }
}
