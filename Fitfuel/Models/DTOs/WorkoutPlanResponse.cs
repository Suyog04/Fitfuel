using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FitFuel.Models.DTOs
{
    public class WorkoutPlanResponse
    {
        [JsonPropertyName("workoutPlan")]
        public Dictionary<string, List<Exercise>> WorkoutPlan { get; set; } = new();
    }
}