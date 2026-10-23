
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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Results
        // Organisers can view all results.
        // Participants can view their own results.
        [HttpGet]
        public async Task<IActionResult> GetResults()
        {
            var results = await _context.Results
                .AsNoTracking()
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.FinishTime,
                    r.FinishingPosition,
                    r.Enrolment.ParticipantId
                })
                .ToListAsync();

            if (!User.IsInRole("Organiser"))
            {
                var userIdClaim = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                results = results
                    .Where(r => r.ParticipantId == userId)
                    .ToList();
            }

            return Ok(results);
        }

        // GET: api/Results/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var result = await _context.Results
                .AsNoTracking()
                .Where(r => r.ResultId == id)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.FinishTime,
                    r.FinishingPosition,
                    r.Enrolment.ParticipantId
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Result not found."
                });
            }

            if (!User.IsInRole("Organiser"))
            {
                var userIdClaim = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized();
                }

                if (result.ParticipantId != userId)
                {
                    return Forbid();
                }
            }

            return Ok(result);
        }


        // POST: api/Results
        // Only organisers can publish results.
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateResult(
            [FromBody] CreateResultRequest request)
        {
            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null)
            {
                return BadRequest(new
                {
                    message = "The specified enrolment does not exist."
                });
            }

            var resultExists = await _context.Results
                .AnyAsync(r =>
                    r.EnrolmentId == request.EnrolmentId);

            if (resultExists)
            {
                return Conflict(new
                {
                    message = "A result already exists for this enrolment."
                });
            }

            if (request.FinishingPosition.HasValue &&
                request.FinishingPosition.Value < 1)
            {
                return BadRequest(new
                {
                    message = "Finishing position must be greater than zero."
                });
            }

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                FinishingPosition = request.FinishingPosition,
                Enrolment = enrolment
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetResult),
                new { id = result.ResultId },
                new
                {
                    result.ResultId,
                    result.EnrolmentId,
                    result.FinishTime,
                    result.FinishingPosition
                });
        }

    }
}
