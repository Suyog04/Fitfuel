using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using FitFuel.Models;
using BCrypt.Net;
using Fitfuel.Models.DTOs;

namespace FitFuel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/Users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict("Email address is already registered");

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = newUser.UserId }, new UserResponse
        {
            UserId = newUser.UserId,
            Name = newUser.Name,
            Email = newUser.Email,
            CreatedAt = newUser.CreatedAt,
            Age = newUser.Age,
            Gender = newUser.Gender,
            HeightCm = newUser.HeightCm,
            WeightKg = newUser.WeightKg,
            TargetWeightKg = newUser.TargetWeightKg,
            Goal = newUser.Goal,
            CalorieEntries = new()
        });
    }

    // PUT: api/Users/{id}/complete-profile
    [HttpPut("{id}/complete-profile")]
    public async Task<IActionResult> CompleteProfile(Guid id, [FromBody] UserCompleteProfileRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found");

        user.Age = request.Age;
        user.Gender = request.Gender;
        user.HeightCm = request.HeightCm;
        user.WeightKg = request.WeightKg;
        user.TargetWeightKg = request.TargetWeightKg;
        user.Goal = request.Goal;

        await _context.SaveChangesAsync();
        return Ok("Profile updated successfully");
    }
    
    // PATCH or PUT: api/Users/{id}/update-profile
    [HttpPut("{id}/update-profile")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound("User not found");

        // Update only provided fields (null checks)
        if (!string.IsNullOrEmpty(request.Name))
            user.Name = request.Name;

        if (request.HeightCm.HasValue)
            user.HeightCm = request.HeightCm.Value;

        if (request.WeightKg.HasValue)
            user.WeightKg = request.WeightKg.Value;

        if (request.TargetWeightKg.HasValue)
            user.TargetWeightKg = request.TargetWeightKg.Value;

        if (!string.IsNullOrEmpty(request.Goal))
            user.Goal = request.Goal;

        if (!string.IsNullOrEmpty(request.Gender))
            user.Gender = request.Gender;

        if (request.Age.HasValue)
            user.Age = request.Age.Value;

        await _context.SaveChangesAsync();

        return Ok("Profile updated successfully");
    }


    // GET: api/Users
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .Select(u => new UserResponse
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                Age = u.Age,
                Gender = u.Gender,
                HeightCm = u.HeightCm,
                WeightKg = u.WeightKg,
                TargetWeightKg = u.TargetWeightKg,
                Goal = u.Goal,
                CalorieEntries = new()
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _context.Users
            .Include(u => u.CalorieEntries)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null) return NotFound();

        var response = new UserResponse
        {
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Age = user.Age,
            Gender = user.Gender,
            HeightCm = user.HeightCm,
            WeightKg = user.WeightKg,
            TargetWeightKg = user.TargetWeightKg,
            Goal = user.Goal,
            CalorieEntries = user.CalorieEntries
                .Select(e => new CalorieEntryResponse
                {
                    EntryId = e.EntryId,
                    FoodItem = e.FoodItem,
                    WeightInGrams = e.WeightInGrams,
                    Meal = e.Meal,
                    EntryTime = e.EntryTime,
                    Calories = e.Calories,
                    Protein = e.Protein,
                    Carbs = e.Carbs,
                    Fats = e.Fats,
                    Fiber = e.Fiber
                }).ToList()
        };

        return Ok(response);
    }
}
