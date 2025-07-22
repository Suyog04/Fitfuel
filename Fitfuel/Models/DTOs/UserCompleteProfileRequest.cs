using System.ComponentModel.DataAnnotations;

namespace Fitfuel.Models.DTOs;

public class UserCompleteProfileRequest
{
    [Range(5, 120)]
    public int Age { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required, Range(50, 300)]
    public double HeightCm { get; set; }

    [Required, Range(20, 300)]
    public double WeightKg { get; set; }

    [Required, Range(20, 300)]
    public double TargetWeightKg { get; set; }

    [Required]
    public string Goal { get; set; } = string.Empty;

    [Required]
    public string FitnessLevel { get; set; } = string.Empty;  // e.g., Beginner, Intermediate, Advanced

    [Required, Range(1, 7)]
    public int Availability { get; set; }  // Number of workout days per week

    [Required]
    public string Equipment { get; set; } = string.Empty;  // e.g., Dumbbells, Resistance Bands, Bodyweight
}