using FitFuel.Models;

public class UserResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? Age { get; set; }
    public string? Gender { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public double? TargetWeightKg { get; set; }
    public string? Goal { get; set; }

    public List<CalorieEntryResponse> CalorieEntries { get; set; }
}