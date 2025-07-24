using FitFuel.Models;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Profile Info
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public double? HeightCm { get; set; }
    public double? WeightKg { get; set; }
    public double? TargetWeightKg { get; set; }
    public string? Goal { get; set; }

    // Workout-specific fields
    public string? FitnessLevel { get; set; }     // Beginner / Intermediate / Advanced
    public int? Availability { get; set; }        // Days per week available
    public string? Equipment { get; set; }        // Comma-separated list
    public string? ActivityLevel { get; set; }    // Sedentary / Light / Moderate / Active / Very Active

    public string Role { get; set; } = "User";   // <--- Default role is User

    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }

    public virtual ICollection<CalorieEntry> CalorieEntries { get; set; } = new List<CalorieEntry>();
}