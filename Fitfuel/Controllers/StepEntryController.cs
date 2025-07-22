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

        // POST: api/StepEntry
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateStep([FromBody] StepEntryRequest request)
        {
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

        // GET: api/StepEntry/user/{userId}?date=2025-07-21
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetStepsByDate(Guid userId, [FromQuery] DateTime? date = null)
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;

            var entry = await _context.StepEntries
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Date == targetDate);

            if (entry == null)
                return NotFound(new { message = $"No step entry found for {targetDate:yyyy-MM-dd}." });

            var response = new StepEntryResponse
            {
                Date = entry.Date,
                StepCount = entry.StepCount
            };

            return Ok(response);
        }

        // GET: api/StepEntry/range/{userId}?start=2025-07-01&end=2025-07-10
        [HttpGet("range/{userId}")]
        public async Task<IActionResult> GetStepsInRange(Guid userId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
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
