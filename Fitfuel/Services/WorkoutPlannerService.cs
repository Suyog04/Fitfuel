using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using FitFuel.Models.DTOs;
using Microsoft.Extensions.Options;
using System;
using FitFuel.Models;

namespace FitFuel.Services
{
    public class WorkoutPlannerService
    {
        private readonly HttpClient _httpClient;

        public WorkoutPlannerService(HttpClient httpClient, IOptions<WorkoutPlannerSettings> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            
            // Optionally check if BaseAddress is set; if not, throw or log
            if (_httpClient.BaseAddress == null)
                throw new ArgumentException("HttpClient.BaseAddress is not set. Check your configuration.");
        }

        public async Task<Dictionary<string, List<string>>?> GetWorkoutPlanAsync(WorkoutRequestDto request)
        {
            // Post to relative endpoint path, for example "workout_planner"
            var response = await _httpClient.PostAsJsonAsync("workout_planner", request);

            if (!response.IsSuccessStatusCode)
                return null;

            var jsonString = await response.Content.ReadAsStringAsync();

            var workoutPlan = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString);

            return workoutPlan;
        }
    }
}