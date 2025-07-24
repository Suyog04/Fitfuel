using System;

namespace FitFuel.Models.DTOs
{
    public class StepEntryResponse
    {
        public DateTime Date { get; set; }
        public int StepCount { get; set; }
        public double StepCaloriesBurnt { get; set; }  // 👈 new field
    }

}