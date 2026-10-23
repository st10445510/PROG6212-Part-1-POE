
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Events
        // Both organisers and participants can view events.
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events
                .AsNoTracking()
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            return Ok(eventItem);
        }

        // POST: api/Events
        // Only organisers can create events.
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(Event eventItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
            {
                return Unauthorized();
            }

            eventItem.EventId = 0;
            eventItem.OrganiserId = organiserId;

            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = eventItem.EventId },
                eventItem);
        }

        // PUT: api/Events/5
        // Only the organiser who owns the event can update it.
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event eventItem)
        {
            if (id != eventItem.EventId)
            {
                return BadRequest(new { message = "Event ID mismatch." });
            }

            var existingEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
            {
                return Unauthorized();
            }

            if (existingEvent.OrganiserId != organiserId)
            {
                return Forbid();
            }

            existingEvent.EventName = eventItem.EventName;
            existingEvent.EventDate = eventItem.EventDate;
            existingEvent.Location = eventItem.Location;
            existingEvent.Description = eventItem.Description;
            existingEvent.Distance = eventItem.Distance;
            existingEvent.EventType = eventItem.EventType;

            await _context.SaveChangesAsync();

            return Ok(existingEvent);
        }

        // DELETE: api/Events/5
        // Only the organiser who owns the event can delete it.
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventItem == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int organiserId))
            {
                return Unauthorized();
            }

            if (eventItem.OrganiserId != organiserId)
            {
                return Forbid();
            }

            _context.Events.Remove(eventItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
