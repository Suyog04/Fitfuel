using FitFuel.Data;
using FitFuel.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using DotNetEnv;

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
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<NutritionService>();

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