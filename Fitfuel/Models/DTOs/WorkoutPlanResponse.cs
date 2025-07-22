namespace FitFuel.Models.DTOs
{
    public class WorkoutPlanResponse
    {
        public Dictionary<string, List<string>> WorkoutPlan { get; set; } = new();
    }
}