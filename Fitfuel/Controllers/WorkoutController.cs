using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public class UserIdRequest
        {
            public Guid UserId { get; set; }
        }

        [HttpPost("workout-plan")]
        public async Task<IActionResult> GetWorkoutPlan([FromBody] UserIdRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound("User not found.");
            
            string fitnessLevel = string.IsNullOrEmpty(user.FitnessLevel) ? "Beginner" : user.FitnessLevel;
            string goal = string.IsNullOrEmpty(user.Goal) ? "Fat Loss" : user.Goal;
            int availability = user.Availability ?? 3;
            string equipment = string.IsNullOrEmpty(user.Equipment) ? "Bodyweight" : user.Equipment;
            int age = user.Age ?? 25;
            string gender = string.IsNullOrEmpty(user.Gender) ? "Male" : user.Gender;
            double height = user.HeightCm ?? 170.0;
            double weight = user.WeightKg ?? 65.0;
            string activityLevel = string.IsNullOrEmpty(user.ActivityLevel) ? "Moderate" : user.ActivityLevel;

            var workoutRequest = new WorkoutRequestDto
            {
                FitnessLevel = fitnessLevel,
                Goal = goal,
                Availability = availability,
                EquipmentStr = equipment,
                Age = age,
                Gender = gender,
                Height = height,
                Weight = weight,
                ActivityLevel = activityLevel
            };

            var postContent = new StringContent(
                JsonSerializer.Serialize(workoutRequest,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Encoding.UTF8,
                "application/json"
            );

            var postResponse =
                await _httpClient.PostAsync("https://luckily-helped-bison.ngrok-free.app/workout_planner", postContent);

            if (!postResponse.IsSuccessStatusCode)
                return StatusCode((int)postResponse.StatusCode, "Failed to send data to ML server");

            var jsonString = await postResponse.Content.ReadAsStringAsync();
            Console.WriteLine("ML server response JSON: " + jsonString);

            var workoutDict = JsonSerializer.Deserialize<Dictionary<string, List<Exercise>>>(jsonString,
                new JsonSerializerOptions
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

            var entry = new PredictedCalorie
            {
                Id = Guid.NewGuid(),
                UserId = user.UserId,
                Date = DateTime.UtcNow.Date,
                Duration = request.Duration,
                HeartRate = request.HeartRate,
                BodyTemp = request.BodyTemp,
                PredictedCalories = prediction.Predicted_Calories
            };

            _context.PredictedCalories.Add(entry);
            await _context.SaveChangesAsync();

            return Ok(prediction);
        }

        [HttpGet("predicted-calories")]
        public async Task<IActionResult> GetPredictedCaloriesByDate(
            [FromQuery] Guid userId,
            [FromQuery(Name = "date")] DateTime? date = null)
        {
            var targetDate = DateTime.SpecifyKind(date?.Date ?? DateTime.UtcNow.Date, DateTimeKind.Utc);

            var entry = await _context.PredictedCalories
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.UserId == userId &&
                    p.Date == targetDate);

            if (entry == null)
            {
                return Ok(new
                {
                    message = "No predicted calorie data found for this user on the specified date."
                });
            }

            return Ok(new
            {
                entry.UserId,
                entry.Date,
                entry.Duration,
                entry.HeartRate,
                entry.BodyTemp,
                entry.PredictedCalories
            });
        }


        [HttpPost("test-post")]
        public IActionResult TestPost() => Ok("POST endpoint working");

        [HttpGet]
        public IActionResult TestGet() => Ok("GET endpoint working");
    }
}
