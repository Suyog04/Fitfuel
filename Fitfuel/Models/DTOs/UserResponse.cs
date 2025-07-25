using FitFuel.Models;
using System;

public class UserResponse
{
    public Guid UserId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? Age { get; set; } = 0;
    public string? Gender { get; set; } = string.Empty;
    public double? HeightCm { get; set; } = 0;
    public double? WeightKg { get; set; } = 0;
    public double? TargetWeightKg { get; set; } = 0;
    public string? Goal { get; set; } = string.Empty;

    public List<CalorieEntryResponse> CalorieEntries { get; set; } = new List<CalorieEntryResponse>();
}