using System;

namespace FitFuel.Models
{
    public class CalorieEntryResponse
    {
        public Guid EntryId { get; set; }
        public Guid UserId { get; set; }
        public string FoodItem { get; set; }
        public double WeightInGrams { get; set; }
        public MealType Meal { get; set; }
        public DateTime EntryTime { get; set; }
        public double Calories { get; set; }
        public double Protein { get; set; }
        public double Carbs { get; set; }
        public double Fats { get; set; }
        public double Fiber { get; set; }
    }
}