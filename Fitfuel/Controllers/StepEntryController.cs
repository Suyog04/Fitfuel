using FitFuel.Data;
using FitFuel.Models;
using FitFuel.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StepEntryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StepEntryController> _logger;

        public StepEntryController(AppDbContext context, ILogger<StepEntryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddOrUpdateStep([FromBody] StepEntryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var date = request.Date.Date;

                var existing = await _context.StepEntries
                    .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.Date == date);

                if (existing != null)
                {
                    existing.StepCount = request.StepCount;
                    _logger.LogInformation($"Updated step entry for user {request.UserId} on {date:yyyy-MM-dd}.");
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
                    _logger.LogInformation($"Added new step entry for user {request.UserId} on {date:yyyy-MM-dd}.");
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Step entry saved successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding/updating step entry.");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(StepEntryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepsByDate(Guid userId, [FromQuery] DateTime? date = null)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving step entry.");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpGet("range/{userId}")]
        [ProducesResponseType(typeof(List<StepEntryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepsInRange(Guid userId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            if (start > end)
                return BadRequest(new { message = "Start date must be before end date." });

            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving step entries in range.");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpGet("last7days/{userId}")]
        [ProducesResponseType(typeof(List<StepEntryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStepsForLast7Days(Guid userId)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var sevenDaysAgo = today.AddDays(-6);

                var entries = await _context.StepEntries
                    .Where(e => e.UserId == userId && e.Date >= sevenDaysAgo && e.Date <= today)
                    .OrderBy(e => e.Date)
                    .Select(e => new StepEntryResponse
                    {
                        Date = e.Date,
                        StepCount = e.StepCount
                    })
                    .ToListAsync();

                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving last 7 days step entries.");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }
    }
}
