using DotNetEnv;
using FitFuel.Data;
using FitFuel.Models;
using FitFuel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Load DB context (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Controllers
builder.Services.AddControllers();

// Enable endpoint explorer
builder.Services.AddEndpointsApiExplorer();

// Enable Swagger docs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FitFuel API",
        Version = "v1",
        Description = "API documentation for the FitFuel fitness tracking app"
    });

    c.UseInlineDefinitionsForEnums();
});

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // ⚠️ Use only for dev. Use .WithOrigins("http://localhost:53925") for Flutter web dev
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register HttpClient for NutritionService
builder.Services.AddHttpClient<NutritionService>();

// Register IHttpClientFactory for general use
builder.Services.AddHttpClient();

// Bind WorkoutPlannerSettings
builder.Services.Configure<WorkoutPlannerSettings>(
    builder.Configuration.GetSection("WorkoutPlanner"));

// Register WorkoutPlannerService with custom base URL
builder.Services.AddHttpClient<WorkoutPlannerService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<WorkoutPlannerSettings>>();
    var baseUrl = options.Value.MlApiUrl;

    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
});

// Register SendGrid email sender
builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();

var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FitFuel API V1");
    c.RoutePrefix = "swagger";
});

// ✅ Use CORS before anything that uses endpoints
app.UseCors("AllowAll");

// Optional: add HTTPS redirection (recommended in production)
// app.UseHttpsRedirection();

app.UseAuthorization();

// Map controllers
app.MapControllers();

// Run the application
app.Run();
