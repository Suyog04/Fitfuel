// Converters.cs

using FitFuel.Models;

namespace FitFuel.Utilities
{
    public static class Converters
    {
        public static CalorieEntryResponse ToResponse(this CalorieEntry entry)
        {
            return new CalorieEntryResponse
            {
                EntryId = entry.EntryId,
                UserId = entry.UserId,
                FoodItem = entry.FoodItem,
                WeightInGrams = entry.WeightInGrams,
                Meal = entry.Meal,
                EntryTime = entry.EntryTime,
                Calories = entry.Calories,
                Protein = entry.Protein,
                Carbs = entry.Carbs,
                Fats = entry.Fats,
                Fiber = entry.Fiber
            };
        }
    }
}