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
}