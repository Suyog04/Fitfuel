using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using FitFuel.Models;
using Fitfuel.Models.DTOs;
using FitFuel.Models.DTOs;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public WorkoutController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("workout-plan")]
        public async Task<IActionResult> GetWorkoutPlan([FromBody] UserIdRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound("User not found.");

            var workoutRequest = new WorkoutRequestDto
            {
                FitnessLevel = user.FitnessLevel ?? "Beginner",
                Goal = user.Goal ?? "",
                Availability = user.Availability ?? 0,
                EquipmentStr = user.Equipment ?? "",
                Age = user.Age ?? 0,
                Gender = user.Gender ?? "",
                Height = user.HeightCm ?? 0,
                Weight = user.WeightKg ?? 0,
                ActivityLevel = user.ActivityLevel ?? "Moderate"
            };

            var postContent = new StringContent(
                JsonSerializer.Serialize(workoutRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json"
            );

            var postResponse = await _httpClient.PostAsync("https://luckily-helped-bison.ngrok-free.app/workout_planner", postContent);

            if (!postResponse.IsSuccessStatusCode)
                return StatusCode((int)postResponse.StatusCode, "Failed to send data to ML server");

            var jsonString = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine("ML server response JSON: " + jsonString);

            var workoutDict = JsonSerializer.Deserialize<Dictionary<string, List<Exercise>>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (workoutDict == null || workoutDict.Count == 0)
                return Ok(new { message = "No workout plan available for the provided inputs." });

            var workoutResponse = new WorkoutPlanResponse
            {
                WorkoutPlan = workoutDict
            };

            return Ok(workoutResponse);
        }

// Define this request DTO for receiving userId
        public class UserIdRequest
        {
            public Guid UserId { get; set; }
        }

        [HttpPost("predict-calories")]
        public async Task<IActionResult> PredictCalories([FromBody] CalorieEstimationRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            if (string.IsNullOrWhiteSpace(user.Gender) ||
                user.WeightKg == null || user.HeightCm == null || user.Age == null)
            {
                return BadRequest(new { message = "User profile is incomplete for calorie prediction." });
            }

            var mlPayload = new Dictionary<string, object>
            {
                ["Gender"] = user.Gender,
                ["Age"] = user.Age,
                ["Height"] = Math.Round(user.HeightCm.Value / 100.0, 2),
                ["Weight"] = Math.Round(user.WeightKg.Value, 2),
                ["Duration"] = request.Duration,
                ["Heart_Rate"] = request.HeartRate,
                ["Body_Temp"] = request.BodyTemp
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(mlPayload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("https://luckily-helped-bison.ngrok-free.app/predict_calories", jsonContent);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Failed to get prediction from ML server");

            var responseString = await response.Content.ReadAsStringAsync();

            var prediction = JsonSerializer.Deserialize<CaloriePredictionResponse>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (prediction == null)
                return StatusCode(500, "Invalid response from ML server");

            return Ok(prediction);
        }



        [HttpPost("test-post")]
        public IActionResult TestPost() => Ok("POST endpoint working");

        [HttpGet]
        public IActionResult TestGet() => Ok("GET endpoint working");
    }
}
