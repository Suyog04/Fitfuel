using System.Collections.Generic;
using System.Linq;

namespace FitFuel.Models
{
    public class MealSummary
    {
        public string MealType { get; set; }
        public double TotalCalories { get; set; }
        public double TotalProtein { get; set; }
        public double TotalCarbs { get; set; }
        public double TotalFats { get; set; }
        public double TotalFiber { get; set; }
        public List<CalorieEntryResponse> Entries { get; set; }
    }
}