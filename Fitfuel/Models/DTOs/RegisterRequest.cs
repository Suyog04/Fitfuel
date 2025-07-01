using System;
using System.ComponentModel.DataAnnotations;

namespace FitFuel.Models.DTOs
{
    public class RegisterRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required, Range(50, 300)] // height in cm
        public double HeightCm { get; set; }

        [Required, Range(20, 300)] // weight in kg
        public double WeightKg { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }
    }
}