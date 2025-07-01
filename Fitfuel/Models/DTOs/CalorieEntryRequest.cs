using System;
using System.ComponentModel.DataAnnotations;
using FitFuel.Models;  // Assuming MealType is declared here

namespace FitFuel.Models.DTOs
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