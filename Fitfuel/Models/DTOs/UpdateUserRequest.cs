namespace Fitfuel.Models.DTOs;

public class UpdateUserRequest
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public float HeightCm { get; set; }
    public float WeightKg { get; set; }
    public float TargetWeightKg { get; set; }
    public string Goal { get; set; }
    public string FitnessLevel { get; set; }
    public int Availability { get; set; }
    public string Equipment { get; set; }
    public string ActivityLevel { get; set; }
}