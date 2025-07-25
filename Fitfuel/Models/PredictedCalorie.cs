public class PredictedCalorie
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }

    public DateTime Date { get; set; }  // Date of prediction, store Date only (no time)
    public double PredictedCalories { get; set; }

    public double Duration { get; set; }
    public int HeartRate { get; set; }
    public double BodyTemp { get; set; }
}