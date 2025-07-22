using System.ComponentModel.DataAnnotations;

namespace FitFuel.Models.DTOs
{
    public class WorkoutRequestDto
    {
        [Required]
        public string Fitness_level { get; set; } = "Beginner";

        public string Goal { get; set; } = "";

        [Range(1, 7, ErrorMessage = "Availability must be between 1 and 7")]
        public int Availability { get; set; }

        [Required]
        public string Equipment_str { get; set; } = "";

        public int Age { get; set; }
        public string Gender { get; set; } = "";

        public double Height { get; set; }
        public double Weight { get; set; }
    }
}