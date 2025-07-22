using Microsoft.AspNetCore.Mvc;
using FitFuel.Models.DTOs;
using System.Text.Json;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutController : ControllerBase
    {
        [HttpGet("workout-plan")]
        public async Task<IActionResult> GetWorkoutPlan()
        {
            var httpClient = new HttpClient();

            // Replace this with your actual ML service URL (local or remote)
            var response = await httpClient.GetAsync("http://localhost:5001/api/ml/workout");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Failed to fetch workout plan");

            var jsonString = await response.Content.ReadAsStringAsync();

            var workoutDict = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);

            var workoutResponse = new WorkoutPlanResponse
            {
                WorkoutPlan = workoutDict ?? new Dictionary<string, List<string>>()
            };

            return Ok(workoutResponse);
        }
    }
}