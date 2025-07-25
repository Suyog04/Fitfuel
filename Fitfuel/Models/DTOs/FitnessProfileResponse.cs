namespace FitFuel.Models.DTOs
{
    public class FitnessProfileResponse
    {
        public string? FitnessLevel { get; set; } = string.Empty;
        public int? Availability { get; set; } = 0;
        public string? Equipment { get; set; } = string.Empty;
        public string? ActivityLevel { get; set; } = string.Empty;
    }
}