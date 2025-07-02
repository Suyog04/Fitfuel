using System.ComponentModel.DataAnnotations;

namespace Fitfuel.Models.DTOs;

public class UserCompleteProfileRequest
{
    [Range(5, 120)]
    public int Age { get; set; }

    [Required]
    public string Gender { get; set; }

    [Required, Range(50, 300)]
    public double HeightCm { get; set; }

    [Required, Range(20, 300)]
    public double WeightKg { get; set; }

    [Required, Range(20, 300)]
    public double TargetWeightKg { get; set; }

    [Required]
    public string Goal { get; set; }
}
