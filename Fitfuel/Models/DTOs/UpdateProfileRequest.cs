using System.ComponentModel.DataAnnotations;

public class UpdateProfileRequest
{
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public double? TargetWeightKg { get; set; }
    public string? Goal { get; set; }

    // Change these from [Required] to optional
    public string? FitnessLevel { get; set; }  // remove [Required]
    
    [Range(1, 7, ErrorMessage = "Availability must be between 1 and 7.")]
    public int? Availability { get; set; }     // make nullable
    
    public string? Equipment { get; set; }
}