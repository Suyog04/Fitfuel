using System.ComponentModel.DataAnnotations;

public class UpdateProfileRequest
{
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public double? TargetWeightKg { get; set; }
    public string? Goal { get; set; }

    public string? FitnessLevel { get; set; }  // Optional

    [Range(1, 7, ErrorMessage = "Availability must be between 1 and 7.")]
    public int? Availability { get; set; }     // Optional with validation

    public string? Equipment { get; set; }

    public string? ActivityLevel { get; set; } // Optional
}