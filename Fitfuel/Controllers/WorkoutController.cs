using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using FitFuel.Models.DTOs;
using FitFuel.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public WorkoutController()
        {
            _httpClient = new HttpClient();
        }

        [HttpPost("workout-plan")]
        public async Task<IActionResult> GetWorkoutPlan([FromBody] WorkoutRequestDto request)
        {
            var postContent = new StringContent(
                JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json"
            );

            var postResponse = await _httpClient.PostAsync("https://599c40a67d7e.ngrok-free.app/workout_planner", postContent);

            if (!postResponse.IsSuccessStatusCode)
                return StatusCode((int)postResponse.StatusCode, "Failed to send data to ML server");

            var jsonString = await postResponse.Content.ReadAsStringAsync();

            // Log raw JSON response for debugging
            Console.WriteLine("ML server response JSON: " + jsonString);

            // Deserialize response JSON into dictionary of workouts
            var workoutDict = JsonSerializer.Deserialize<Dictionary<string, List<Exercise>>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (workoutDict == null || workoutDict.Count == 0)
            {
                return Ok(new { message = "No workout plan available for the provided inputs." });
            }

            var workoutResponse = new WorkoutPlanResponse
            {
                WorkoutPlan = workoutDict
            };

            return Ok(workoutResponse);
        }

        [HttpPost("test-post")]
        public IActionResult TestPost()
        {
            return Ok("POST endpoint working");
        }

        [HttpGet]
        public IActionResult TestGet()
        {
            return Ok("GET endpoint working");
        }
    }
}
