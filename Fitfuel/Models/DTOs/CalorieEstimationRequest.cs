namespace Fitfuel.Models.DTOs;

public class CalorieEstimationRequest
{
    public Guid UserId { get; set; }
    public double Duration { get; set; }
    public int HeartRate { get; set; }
    public double BodyTemp { get; set; }
}
