
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        // Organisers and participants can view categories.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/Categories/5
        // View one category.
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(category);
        }

        // POST: api/Categories
        // Only organisers can create categories.
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            Category category)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == category.EventId);

            if (eventItem == null)
            {
                return BadRequest(new
                {
                    message = "The specified event does not exist."
                });
            }

            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                return BadRequest(new
                {
                    message = "Category name is required."
                });
            }

            category.CategoryId = 0;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.CategoryId },
                category);
        }
    }
}
