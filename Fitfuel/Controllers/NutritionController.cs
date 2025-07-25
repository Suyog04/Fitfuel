using FitFuel.Data;
using Fitfuel.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using Fitfuel.Models.DTOs;
using Fitfuel.Models;

namespace YourProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NutritionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NutritionController> _logger;
        private readonly HttpClient _httpClient;

        public NutritionController(AppDbContext context, ILogger<NutritionController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("generate-nutrition-goal/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateNutritionGoalPlan(Guid userId)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                if (user.WeightKg == null || user.HeightCm == null || user.Age == null || string.IsNullOrEmpty(user.Gender))
                    return BadRequest(new { message = "Incomplete user profile for nutrition planning." });

                var mlRequest = new NutritionGoalPlanRequest
                {
                    FitnessLevel = user.FitnessLevel ?? "Beginner",
                    Goal = user.Goal ?? "Fat Loss",
                    ActivityLevel = user.ActivityLevel ?? "Moderate",
                    Age = user.Age.Value,
                    Gender = user.Gender,
                    Height = (int)user.HeightCm.Value,
                    Weight = user.WeightKg.Value
                };

                var mlApiUrl = "https://luckily-helped-bison.ngrok-free.app/nutrition_plan";

                var response = await _httpClient.PostAsJsonAsync(mlApiUrl, mlRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ML API responded with {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

                    // Return ML API error details to client for debugging
                    return StatusCode((int)response.StatusCode, new
                    {
                        message = "ML server error",
                        statusCode = response.StatusCode,
                        details = errorContent
                    });
                }

                var plan = await response.Content.ReadFromJsonAsync<object>();
                return Ok(plan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating nutrition goal plan.");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }
    }
}
