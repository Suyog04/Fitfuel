using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using FitFuel.Models;
using BCrypt.Net;
using Fitfuel.Models.DTOs;
using FitFuel.Models.DTOs;
using FitFuel.Services;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly WorkoutPlannerService _workoutPlannerService;

        public UsersController(AppDbContext context, WorkoutPlannerService workoutPlannerService)
        {
            _context = context;
            _workoutPlannerService = workoutPlannerService;
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
                // Other properties (Age, Gender, etc.) can be set later
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = newUser.UserId }, new UserResponse
            {
                UserId = newUser.UserId,
                Name = newUser.Name ?? "",
                Email = newUser.Email ?? "",
                CreatedAt = newUser.CreatedAt,
                Age = newUser.Age ?? 0,
                Gender = newUser.Gender ?? "",
                HeightCm = newUser.HeightCm ?? 0,
                WeightKg = newUser.WeightKg ?? 0,
                TargetWeightKg = newUser.TargetWeightKg ?? 0,
                Goal = newUser.Goal ?? "",
                CalorieEntries = new List<CalorieEntryResponse>()
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
            return Ok("Profile has been successfully completed");
        }

    
        [HttpPut("{id}/update-profile")]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] UpdateProfileRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            // Update fields if present (excluding Name)
            if (request.HeightCm.HasValue) user.HeightCm = request.HeightCm;
            if (request.WeightKg.HasValue) user.WeightKg = request.WeightKg;
            if (request.TargetWeightKg.HasValue) user.TargetWeightKg = request.TargetWeightKg;
            if (!string.IsNullOrWhiteSpace(request.Goal)) user.Goal = request.Goal;
            if (!string.IsNullOrWhiteSpace(request.Gender)) user.Gender = request.Gender;
            if (request.Age.HasValue) user.Age = request.Age;
            if (!string.IsNullOrWhiteSpace(request.FitnessLevel)) user.FitnessLevel = request.FitnessLevel;
            if (request.Availability.HasValue) user.Availability = request.Availability;
            if (!string.IsNullOrWhiteSpace(request.Equipment)) user.Equipment = request.Equipment;
            if (!string.IsNullOrWhiteSpace(request.ActivityLevel)) user.ActivityLevel = request.ActivityLevel;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
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
                    Name = u.Name ?? "",
                    Email = u.Email ?? "",
                    CreatedAt = u.CreatedAt,
                    Age = u.Age ?? 0,
                    Gender = u.Gender ?? "",
                    HeightCm = u.HeightCm ?? 0,
                    WeightKg = u.WeightKg ?? 0,
                    TargetWeightKg = u.TargetWeightKg ?? 0,
                    Goal = u.Goal ?? "",
                    CalorieEntries = new List<CalorieEntryResponse>()
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

            if (user == null)
                return NotFound();

            var response = new UserResponse
            {
                UserId = user.UserId,
                Name = user.Name ?? "",
                Email = user.Email ?? "",
                CreatedAt = user.CreatedAt,
                Age = user.Age ?? 0,
                Gender = user.Gender ?? "",
                HeightCm = user.HeightCm ?? 0,
                WeightKg = user.WeightKg ?? 0,
                TargetWeightKg = user.TargetWeightKg ?? 0,
                Goal = user.Goal ?? "",
                CalorieEntries = user.CalorieEntries
                    .Select(e => new CalorieEntryResponse
                    {
                        EntryId = e.EntryId,
                        FoodItem = e.FoodItem ?? "",
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

        
        [HttpGet("{id}/fitness-profile")]
        public async Task<IActionResult> GetFitnessProfile(
            Guid id,
            [FromQuery] DateTime? date = null)  // Optional date parameter with default
        {
            var targetDate = DateTime.SpecifyKind(date?.Date ?? DateTime.UtcNow.Date, DateTimeKind.Utc);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
                return NotFound(new { message = "User not found." });
    
            var profile = new FitnessProfileResponse
            {
                FitnessLevel = user.FitnessLevel ?? "",
                Availability = user.Availability ?? 0,
                Equipment = user.Equipment ?? "",
                ActivityLevel = user.ActivityLevel ?? ""
            };

            return Ok(profile);
        }
    }
}
