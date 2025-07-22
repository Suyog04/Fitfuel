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

            user.FitnessLevel = request.FitnessLevel;
            user.Availability = request.Availability;
            user.Equipment = request.Equipment;

            await _context.SaveChangesAsync();
            return Ok("Profile updated successfully");
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

            await _context.SaveChangesAsync();

            var workoutRequest = new WorkoutRequestDto
            {
                Fitness_level = user.FitnessLevel ?? "Beginner",
                Goal = user.Goal ?? "",
                Availability = user.Availability ?? 0,
                Equipment_str = user.Equipment ?? "",
                Age = user.Age ?? 0,
                Gender = user.Gender ?? "",
                Height = user.HeightCm ?? 0,
                Weight = user.WeightKg ?? 0
            };

            try
            {
                var workoutPlan = await _workoutPlannerService.GetWorkoutPlanAsync(workoutRequest);

                if (workoutPlan == null)
                    return StatusCode(500, "Failed to fetch workout plan from external service.");

                return Ok(new
                {
                    message = "Profile updated and workout plan received.",
                    workoutPlan
                });
            }
            catch (Exception ex)
            {
                // Log ex.Message if you have logger, else just return error message
                return StatusCode(500, $"Error calling ML API: {ex.Message}");
            }
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

            if (user == null)
                return NotFound();

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
}
