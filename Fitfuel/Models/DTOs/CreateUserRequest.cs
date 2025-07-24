namespace Fitfuel.Models.DTOs;

public class CreateUserRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public double? TargetWeightKg { get; set; }
    public string? Goal { get; set; }
    public string? FitnessLevel { get; set; }
    public int? Availability { get; set; }
    public string? Equipment { get; set; }
    public string? ActivityLevel { get; set; }
}