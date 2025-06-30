using FitFuel.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public Guid UserId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ✅ New fields
    [Range(50, 300)] // in cm
    public double HeightCm { get; set; }

    [Range(20, 300)] // in kg
    public double WeightKg { get; set; }

    [Range(5, 120)]
    public int Age { get; set; }

    public virtual ICollection<CalorieEntry> CalorieEntries { get; set; } = new List<CalorieEntry>();
}