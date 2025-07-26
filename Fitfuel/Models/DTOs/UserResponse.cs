using FitFuel.Models;
using System;

public class UserResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int Age { get; set; } = 0;           
    public string Gender { get; set; } = "";     
    public double HeightCm { get; set; } = 0.0;  
    public double WeightKg { get; set; } = 0.0; 
    public double TargetWeightKg { get; set; } = 0.0;
    public string Goal { get; set; } = "";      
    public List<CalorieEntryResponse> CalorieEntries { get; set; } = new List<CalorieEntryResponse>();
}