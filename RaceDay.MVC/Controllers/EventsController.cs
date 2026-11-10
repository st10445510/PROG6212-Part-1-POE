
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
                return RedirectToAction("Login", "Auth");

            var client = GetApiClient();

            var response = await client.GetAsync("api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load events. Please try again.";
                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            return View(events ?? new List<EventViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Index", "Events");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (HttpContext.Session.GetString("Role") != "Organiser")
                return RedirectToAction("Index", "Events");

            if (!ModelState.IsValid)
                return View(model);

            var client = GetApiClient();

            var response = await client.PostAsJsonAsync("api/Events", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event created successfully!";
                return RedirectToAction("Index");
            }

            var errorText = await response.Content.ReadAsStringAsync();

            ViewBag.Error = $"API Error ({(int)response.StatusCode}): {errorText}";

            return View(model);
        }
    }
}
