using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FitFuel.Models;

namespace FitFuel.Services;

public class NutritionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public NutritionService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<NutritionResult?> GetNutritionDataAsync(string foodItem, double weightInGrams)
    {
        var requestUrl = "https://trackapi.nutritionix.com/v2/natural/nutrients";

        // Format query string: e.g. "banana 100 grams"
        var query = $"{foodItem} {weightInGrams} grams";

        var body = new { query };
        var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        // Clear old headers and set new ones for this API request
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-app-id", _config["Nutritionix:AppId"]);
        _httpClient.DefaultRequestHeaders.Add("x-app-key", _config["Nutritionix:AppKey"]);
        _httpClient.DefaultRequestHeaders.Add("x-remote-user-id", "0");

        // Send request
        var response = await _httpClient.PostAsync(requestUrl, content);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($" Nutrition API error: {response.StatusCode} - {error}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("foods", out var foods) || foods.GetArrayLength() == 0)
        {
            Console.WriteLine(" Nutrition API returned no foods.");
            return null;
        }

        var food = foods[0];

        // Extract nutrients and return
        return new NutritionResult
        {
            Calories = food.GetProperty("nf_calories").GetDouble(),
            Protein = food.GetProperty("nf_protein").GetDouble(),
            Carbs = food.GetProperty("nf_total_carbohydrate").GetDouble(),
            Fats = food.GetProperty("nf_total_fat").GetDouble(),
            Fiber = food.GetProperty("nf_dietary_fiber").GetDouble()
        };
    }
}

// Model to hold the nutrition results
public class NutritionResult
{
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fats { get; set; }
    public double Fiber { get; set; }
}
