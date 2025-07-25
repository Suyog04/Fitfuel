using System.Text.Json.Serialization;

public class NutritionGoalPlanRequest
{
    [JsonPropertyName("fitness_level")]
    public string FitnessLevel { get; set; }

    [JsonPropertyName("goal")]
    public string Goal { get; set; }

    [JsonPropertyName("activity_level")]
    public string ActivityLevel { get; set; }

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("weight")]
    public double Weight { get; set; }
}