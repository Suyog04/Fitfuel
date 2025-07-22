using FitFuel.Data;
using FitFuel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using FitFuel.Models;
using Microsoft.Extensions.Options;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add Controllers and HTTP clients
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add HttpClient for NutritionService (typed client)
builder.Services.AddHttpClient<NutritionService>();

// Bind WorkoutPlannerSettings from config (you must add it to appsettings.json)
builder.Services.Configure<WorkoutPlannerSettings>(builder.Configuration.GetSection("WorkoutPlanner"));

// Add HttpClient for WorkoutPlannerService with base address from config
builder.Services.AddHttpClient<WorkoutPlannerService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<WorkoutPlannerSettings>>();
    var baseUrl = options.Value.MlApiUrl;

    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        client.BaseAddress = new Uri(baseUrl);
    }
});

// Add SendGrid email sender
builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();

// Add Swagger with metadata
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FitFuel API",
        Version = "v1",
        Description = "API documentation for the FitFuel fitness tracking app"
    });

    // Optional: show enums as strings
    c.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FitFuel API V1");
    c.RoutePrefix = "swagger"; // so you visit /swagger
});

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
