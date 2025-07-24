namespace FitFuel.Models.DTOs
{
    public class MacroBreakdownDto
    {
        public string MacroType { get; set; } = string.Empty;
        public double Grams { get; set; }
        public double Calories { get; set; }
        public double PercentageOfTotalCalories { get; set; }
    }
}