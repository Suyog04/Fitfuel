using System.ComponentModel.DataAnnotations;
using FitFuel.Models;

namespace FitFuel.Models
{
    public class CalorieEntryRequest
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public string FoodItem { get; set; }
        
        [Required]
        [Range(1, 5000)]
        public double WeightInGrams { get; set; }
        
        [Required]
        public MealType Meal { get; set; }
    }
}