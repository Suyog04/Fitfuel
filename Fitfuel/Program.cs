using FitFuel.Data;
using FitFuel.Services;
using Microsoft.EntityFrameworkCore;
using DotNetEnv; 


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL provider
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// this add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<NutritionService>();

// ✅ Register SendGrid email sender
builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();