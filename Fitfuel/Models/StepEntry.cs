using System;

namespace FitFuel.Models
{
    public class StepEntry
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public DateTime Date { get; set; } // Just the date, no time
        public int StepCount { get; set; }

        // Optional: Navigation property
        public User? User { get; set; }
    }
}