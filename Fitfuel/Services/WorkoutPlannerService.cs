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

            if (_httpClient.BaseAddress == null)
                throw new ArgumentException("HttpClient.BaseAddress is not set. Check your configuration.");
        }

        public async Task<Dictionary<string, List<string>>?> GetWorkoutPlanAsync(WorkoutRequestDto request)
        {
            try
            {
                var jsonRequest = JsonSerializer.Serialize(request);
                Console.WriteLine($"Sending JSON to ML API: {jsonRequest}");

                var response = await _httpClient.PostAsJsonAsync("workout_planner", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ML API error response ({(int)response.StatusCode}): {errorContent}");
                    throw new Exception($"ML API returned error {(int)response.StatusCode} ({response.ReasonPhrase}): {errorContent}");
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var workoutPlan = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonString, options);

                if (workoutPlan == null)
                {
                    throw new Exception("ML API returned null or invalid workout plan JSON.");
                }

                Console.WriteLine("Received workout plan from ML API successfully.");
                return workoutPlan;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in calling ML API: {ex.Message}");
                throw;
            }
        }
    }
}
