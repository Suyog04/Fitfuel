using System;

namespace FitFuel.Models.DTOs
{
    public class StepEntryRequest
    {
        public Guid UserId { get; set; }
        public int StepCount { get; set; }
        public DateTime Date { get; set; }  // Flutter will send this
    }
}