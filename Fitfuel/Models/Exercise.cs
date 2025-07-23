using System.Text.Json.Serialization;

namespace FitFuel.Models
{
    public class Exercise
    {
        [JsonPropertyName("exercise_name")]
        public string ExerciseName { get; set; } = string.Empty;

        [JsonPropertyName("primary_muscle")]
        public string PrimaryMuscle { get; set; } = string.Empty;

        [JsonPropertyName("sets")]
        public int Sets { get; set; }

        [JsonPropertyName("reps")]
        public int Reps { get; set; }

        [JsonPropertyName("rest")]
        public string Rest { get; set; } = string.Empty;

        [JsonPropertyName("intensity")]
        public string Intensity { get; set; } = string.Empty;
    }
}