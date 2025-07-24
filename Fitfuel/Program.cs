using DotNetEnv;
using FitFuel.Data;
using FitFuel.Models;
using FitFuel.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

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

    // Optionally: Add JWT Authentication to Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme 
            { 
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" 
                }
            },
            new string[] {}
        }
    });
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

// ===== Add JWT Authentication =====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

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

// ===== Add Authentication middleware =====
app.UseAuthentication();  // Must come before UseAuthorization

app.UseAuthorization();

// Optional: add HTTPS redirection (recommended in production)
// app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

// Run the application
app.Run();
