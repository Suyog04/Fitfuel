using System.ComponentModel.DataAnnotations;

namespace Fitfuel.Models.DTOs;

public class UpdateProfileRequest
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }  // optional to update

    [Range(0, 300)]
    public double? HeightCm { get; set; }  // optional

    [Range(0, 300)]
    public double? WeightKg { get; set; }  // optional

    [Range(0, 300)]
    public double? TargetWeightKg { get; set; }  // optional

    public string? Goal { get; set; }  // optional

    public string? Gender { get; set; }  // optional

    [Range(0, 150)]
    public int? Age { get; set; }  // optional

    [StringLength(50)]
    public string? FitnessLevel { get; set; }  // optional (e.g., Beginner, Intermediate, Advanced)

    [Range(1, 7)]
    public int? Availability { get; set; }  // optional, days per week

    [StringLength(200)]
    public string? Equipment { get; set; }  // optional, e.g., Dumbbells, Resistance Bands
}