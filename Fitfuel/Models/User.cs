
using FitFuel.Models;

public class User
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } // Add this
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<CalorieEntry> CalorieEntries { get; set; }
}