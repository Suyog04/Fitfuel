using System;
using System.ComponentModel.DataAnnotations;

namespace FitFuel.Models.DTOs
{
    public class StepEntryRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "StepCount must be zero or positive.")]
        public int StepCount { get; set; }
    }
}