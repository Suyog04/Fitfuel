using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using FitFuel.Models;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;

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
        {
            return BadRequest(ModelState);
        }

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Conflict("Email address is already registered");
        }

        // Hash password with secure work factor
        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        // Return response without password hash
        return CreatedAtAction(nameof(GetById), new { id = newUser.UserId }, new UserResponse
        {
            UserId = newUser.UserId,
            Name = newUser.Name,
            Email = newUser.Email,
            CreatedAt = newUser.CreatedAt
        });
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
                CreatedAt = u.CreatedAt
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
            .Select(u => new UserResponse
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                CreatedAt = u.CreatedAt,
                CalorieEntries = u.CalorieEntries.Select(e => new CalorieEntryResponse
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
            })
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null) return NotFound();

        return Ok(user);
    }
}

// DTO Classes
public class UserCreateRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }
    
    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}

public class UserResponse
{
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CalorieEntryResponse> CalorieEntries { get; set; }
}

public class CalorieEntryResponse
{
    public Guid EntryId { get; set; }
    public string FoodItem { get; set; }
    public double WeightInGrams { get; set; }
    public MealType Meal { get; set; }
    public DateTime EntryTime { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fats { get; set; }
    public double Fiber { get; set; }
}