using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FitFuel.Models.DTOs
{
    public class WorkoutRequestDto
    {
        [Required]
        [JsonPropertyName("fitness_level")]
        public string FitnessLevel { get; set; } = "Beginner";

        [JsonPropertyName("goal")]
        public string Goal { get; set; } = "";

        [Range(1, 7, ErrorMessage = "Availability must be between 1 and 7")]
        [JsonPropertyName("availability")]
        public int Availability { get; set; }

        [Required]
        [JsonPropertyName("equipment_str")]
        public string EquipmentStr { get; set; } = "";

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = "";

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("weight")]
        public double Weight { get; set; }

        [JsonPropertyName("activity_level")]
        public string ActivityLevel { get; set; } = "Moderate";
    }
}