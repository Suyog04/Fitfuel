
namespace FitFuel.Models.DTOs;

public class WorkoutRequestDto
{
    public string Fitness_level { get; set; } = "Beginner";
    public string Goal { get; set; } = "";
    public int Availability { get; set; }
    public string Equipment_str { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
    public double Height { get; set; }
    public double Weight { get; set; }
}