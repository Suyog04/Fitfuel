using FitFuel.Models;
using System;
using System.Collections.Generic;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public DateTime DateOfBirth { get; set; }  // NEW

    public virtual ICollection<CalorieEntry> CalorieEntries { get; set; } = new List<CalorieEntry>();
}