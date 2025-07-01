using System.Collections.Generic;

namespace FitFuel.Models
{
    public class MealSummary
    {
        public string MealType { get; set; } = string.Empty;  // or MealType enum if you have one

        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFats { get; set; }
        public double TotalFiber { get; set; }

        public List<CalorieEntryResponse> Entries { get; set; } = new List<CalorieEntryResponse>();
    }
}