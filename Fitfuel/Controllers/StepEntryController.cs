using FitFuel.Data;
using FitFuel.Models;
using FitFuel.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StepEntryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StepEntryController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds or updates the step count for a specific user on a specific date.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddOrUpdateStep([FromBody] StepEntryRequest request)
        {
            if (request.UserId == Guid.Empty || request.StepCount < 0)
                return BadRequest(new { message = "Invalid input." });

            var date = request.Date.Date;

            var existing = await _context.StepEntries
                .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.Date == date);

            if (existing != null)
            {
                existing.StepCount = request.StepCount;
            }
            else
            {
                var newEntry = new StepEntry
                {
                    Id = Guid.NewGuid(),
                    UserId = request.UserId,
                    Date = date,
                    StepCount = request.StepCount
                };

                _context.StepEntries.Add(newEntry);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Step entry saved successfully." });
        }

        /// <summary>
        /// Gets the step count of a user for a specific date.
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(StepEntryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStepsByDate(Guid userId, [FromQuery] DateTime? date = null)
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            var entry = await _context.StepEntries
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == targetDate);

            if (entry == null)
                return NotFound(new { message = $"No step entry found for {targetDate:yyyy-MM-dd}." });

            return Ok(new StepEntryResponse
            {
                Date = entry.Date,
                StepCount = entry.StepCount
            });
        }

        /// <summary>
        /// Gets step counts of a user in a date range.
        /// </summary>
        [HttpGet("range/{userId}")]
        [ProducesResponseType(typeof(List<StepEntryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStepsInRange(Guid userId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
                return BadRequest(new { message = "Start date must be before end date." });

            var entries = await _context.StepEntries
                .Where(e => e.UserId == userId && e.Date >= start.Date && e.Date <= end.Date)
                .OrderBy(e => e.Date)
                .Select(e => new StepEntryResponse
                {
                    Date = e.Date,
                    StepCount = e.StepCount
                })
                .ToListAsync();

            return Ok(entries);
        }
    }
}
