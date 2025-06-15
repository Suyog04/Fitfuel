using FitFuel.Data;
using FitFuel.Models;
using FitFuel.Services;
using FitFuel.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalorieEntriesController(AppDbContext context, NutritionService nutrition) : ControllerBase
    {
        private readonly AppDbContext _context = context;
        private readonly NutritionService _nutrition = nutrition;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CalorieEntryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify user exists
            if (!await _context.Users.AnyAsync(u => u.UserId == request.UserId))
            {
                return BadRequest($"User with ID {request.UserId} does not exist");
            }

            var nutritionData = await _nutrition.GetNutritionDataAsync(
                request.FoodItem, 
                request.WeightInGrams
            );

            if (nutritionData == null)
            {
                return BadRequest("Could not retrieve nutrition data");
            }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entry = await _context.CalorieEntries
                .FirstOrDefaultAsync(e => e.EntryId == id);
            
            if (entry == null) return NotFound();
            
            return Ok(entry.ToResponse());
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entries = await _context.CalorieEntries
                .Select(e => e.ToResponse())
                .ToListAsync();
            
            return Ok(entries);
        }
        
        [HttpGet("summary/{userId}")]
        public async Task<IActionResult> GetNutritionSummary(Guid userId, [FromQuery] DateTime? date = null)
        {
            var targetDate = date?.Date ?? DateTime.UtcNow.Date;
            
            var entries = await _context.CalorieEntries
                .Where(e => e.UserId == userId && e.EntryTime.Date == targetDate)
                .ToListAsync();

            var summary = entries
                .GroupBy(e => e.Meal)
                .Select(g => new MealSummary
                {
                    MealType = g.Key.ToString(),
                    TotalCalories = g.Sum(e => e.Calories),
                    TotalProtein = g.Sum(e => e.Protein),
                    TotalCarbs = g.Sum(e => e.Carbs),
                    TotalFats = g.Sum(e => e.Fats),
                    TotalFiber = g.Sum(e => e.Fiber),
                    Entries = g.Select(e => e.ToResponse()).ToList()
                })
                .OrderBy(s => s.MealType);

            return Ok(summary);
        }
    }
}