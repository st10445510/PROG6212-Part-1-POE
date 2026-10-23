
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrolmentsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Enrolments
        // Organisers can view all enrolments.
        // Participants can view their own enrolments.
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var query = _context.Enrolments
                .AsNoTracking();

            if (!User.IsInRole("Organiser"))
            {
                query = query.Where(e =>
                    e.ParticipantId == userId);
            }

            var enrolments = await query
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.CategoryId,
                    e.EnrolmentDate,
                    e.Status,
                    CategoryName = e.Category.CategoryName
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/Enrolments/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var enrolment = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.EnrolmentId == id)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.ParticipantId,
                    e.CategoryId,
                    e.EnrolmentDate,
                    e.Status,
                    CategoryName = e.Category.CategoryName
                })
                .FirstOrDefaultAsync();

            if (enrolment == null)
            {
                return NotFound(new
                {
                    message = "Enrolment not found."
                });
            }

            if (!User.IsInRole("Organiser") &&
                enrolment.ParticipantId != userId)
            {
                return Forbid();
            }

            return Ok(enrolment);
        }

        // POST: api/Enrolments
        // Participants can enrol themselves in a category.
        [Authorize(Roles = "Participant")]
        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] Enrolment enrolment)
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized();
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.CategoryId == enrolment.CategoryId);

            if (!categoryExists)
            {
                return BadRequest(new
                {
                    message = "The specified category does not exist."
                });
            }

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == userId &&
                    e.CategoryId == enrolment.CategoryId);

            if (alreadyEnrolled)
            {
                return Conflict(new
                {
                    message = "You are already enrolled in this category."
                });
            }

            enrolment.EnrolmentId = 0;
            enrolment.ParticipantId = userId;
            enrolment.EnrolmentDate = DateOnly.FromDateTime(
                DateTime.Today);
            enrolment.Status = "Pending";
            enrolment.Participant = null!;
            enrolment.Category = null!;
            enrolment.Result = null;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEnrolment),
                new { id = enrolment.EnrolmentId },
                new
                {
                    enrolment.EnrolmentId,
                    enrolment.ParticipantId,
                    enrolment.CategoryId,
                    enrolment.EnrolmentDate,
                    enrolment.Status
                });
        }
    }
}
