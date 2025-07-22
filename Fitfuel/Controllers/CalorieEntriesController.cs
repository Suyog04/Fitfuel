using FitFuel.Data;
using FitFuel.Models;
using FitFuel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using FitFuel.Models.DTOs;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalorieEntriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NutritionService _nutrition;

        public CalorieEntriesController(AppDbContext context, NutritionService nutrition)
        {
            _context = context;
            _nutrition = nutrition;
        }

        // ✅ CHECK NUTRITION INFO BEFORE LOGGING
        [HttpGet("check-nutrition")]
        public async Task<IActionResult> CheckNutritionApi([FromQuery] string foodItem, [FromQuery] double weight)
        {
            if (string.IsNullOrWhiteSpace(foodItem) || weight <= 0)
            {
                return BadRequest("Please provide a valid food item and a weight greater than 0.");
            }

            var result = await _nutrition.GetNutritionDataAsync(foodItem, weight);

            if (result == null)
            {
                return StatusCode(502, "Nutrition API is not responding or returned no data.");
            }

            return Ok(new
            {
                status = "Nutrition data retrieved successfully",
                food = foodItem,
                weight = weight,
                nutrition = result
            });
        }

        // ✅ CREATE ENTRY
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CalorieEntryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _context.Users.AnyAsync(u => u.UserId == request.UserId))
                return BadRequest($"User with ID {request.UserId} does not exist");

            var nutritionData = await _nutrition.GetNutritionDataAsync(
                request.FoodItem,
                request.WeightInGrams
            );

            if (nutritionData == null)
                return BadRequest("Could not retrieve nutrition data");

            var newEntry = new CalorieEntry
            {
                EntryId = Guid.NewGuid(),
                UserId = request.UserId,
                FoodItem = request.FoodItem,
                WeightInGrams = request.WeightInGrams,
                Meal = request.Meal,
                EntryTime = DateTime.UtcNow,
                Calories = nutritionData.Calories,
                Protein = nutritionData.Protein,
                Carbs = nutritionData.Carbs,
                Fats = nutritionData.Fats,
                Fiber = nutritionData.Fiber
            };

            _context.CalorieEntries.Add(newEntry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newEntry.EntryId }, newEntry.ToResponse());
        }

        // ✅ FIXED: ENSURE UTC KIND WHEN FILTERING DATE
        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetNutritionSummary(Guid userId, [FromQuery] DateTime? date = null)
        {
            // Use UTC date or provided date truncated to date only, then specify UTC kind
            var rawDate = date?.Date ?? DateTime.UtcNow.Date;
            var targetDate = DateTime.SpecifyKind(rawDate, DateTimeKind.Utc);

            var startDate = targetDate;
            var endDate = targetDate.AddDays(1); // still UTC

            // Filter entries for the date range
            var entries = await _context.CalorieEntries
                .Where(e => e.UserId == userId && e.EntryTime >= startDate && e.EntryTime < endDate)
                .ToListAsync();

            if (!entries.Any())
                return NotFound(new { message = $"No calorie entries found for user on {targetDate:yyyy-MM-dd}." });

            // Group entries by meal type and calculate nutrition summary
            var summary = entries
                .GroupBy(e => e.Meal)
                .Select(g => new MealSummary
                {
                    MealType = Enum.GetName(typeof(MealType), g.Key) ?? g.Key.ToString(),
                    TotalCalories = g.Sum(e => e.Calories),
                    TotalProtein = g.Sum(e => e.Protein),
                    TotalCarbs = g.Sum(e => e.Carbs),
                    TotalFats = g.Sum(e => e.Fats),
                    TotalFiber = g.Sum(e => e.Fiber),
                    Entries = g.OrderBy(e => e.EntryTime)
                        .Select(e => e.ToResponse())
                        .ToList()
                })
                .OrderBy(s => Enum.Parse<MealType>(s.MealType))
                .ToList();

            return Ok(summary);
        }

        // ✅ GET BY ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entry = await _context.CalorieEntries.FirstOrDefaultAsync(e => e.EntryId == id);
            if (entry == null) return NotFound();
            return Ok(entry.ToResponse());
        }

        // ✅ GET ALL ENTRIES
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entries = await _context.CalorieEntries
                .Select(e => e.ToResponse())
                .ToListAsync();

            return Ok(entries);
        }
        [HttpGet("summary/total/{userId}")]
        public async Task<IActionResult> GetTotalSummary(Guid userId, [FromQuery] DateTime? date = null)
        {
            var rawDate = date?.Date ?? DateTime.UtcNow.Date;
            var targetDate = DateTime.SpecifyKind(rawDate, DateTimeKind.Utc);

            var startDate = targetDate;
            var endDate = targetDate.AddDays(1); // for full day range

            var entries = await _context.CalorieEntries
                .Where(e => e.UserId == userId && e.EntryTime >= startDate && e.EntryTime < endDate)
                .ToListAsync();

            if (!entries.Any())
            {
                return NotFound(new { message = $"No calorie entries found for user on {targetDate:yyyy-MM-dd}." });
            }

            var totalSummary = new
            {
                Date = targetDate.ToString("yyyy-MM-dd"),
                TotalCalories = entries.Sum(e => e.Calories),
                TotalProtein = entries.Sum(e => e.Protein),
                TotalCarbs = entries.Sum(e => e.Carbs),
                TotalFats = entries.Sum(e => e.Fats),
                TotalFiber = entries.Sum(e => e.Fiber)
            };

            return Ok(totalSummary);
        }

    }
}
