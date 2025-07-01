using FitFuel.Models;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public DateTime DateOfBirth { get; set; }

    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }


    public virtual ICollection<CalorieEntry> CalorieEntries { get; set; } = new List<CalorieEntry>();
}