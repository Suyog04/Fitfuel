namespace FitFuel.Models;

public class UserResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CalorieEntryResponse> CalorieEntries { get; set; } = new();
}