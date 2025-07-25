using System;

namespace FitFuel.Models
{
    public class CalorieEntryResponse
    {
        public Guid EntryId { get; set; } = Guid.Empty;
        public Guid UserId { get; set; } = Guid.Empty;
        public string FoodItem { get; set; } = string.Empty;
        public double WeightInGrams { get; set; } = 0.0;
        public MealType Meal { get; set; } = MealType.Breakfast; // assuming Breakfast as default
        public DateTime EntryTime { get; set; } = DateTime.MinValue;
        public double Calories { get; set; } = 0.0;
        public double Protein { get; set; } = 0.0;
        public double Carbs { get; set; } = 0.0;
        public double Fats { get; set; } = 0.0;
        public double Fiber { get; set; } = 0.0;
    }
}