using System;
using System.Collections.Generic;

namespace FitFuel.Models.DTOs
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public double HeightCm { get; set; }
        public double WeightKg { get; set; }

        public int Age { get; set; } // Dynamically calculated

        public List<CalorieEntryResponse> CalorieEntries { get; set; } = new();
    }
}