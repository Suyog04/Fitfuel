using FitFuel.Data;
using FitFuel.Models;
using Fitfuel.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("admin/users")]
   // [Authorize(Roles = "Admin")]  // Only allow users with Admin role
    public class AdminUserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailSender _emailSender;

        public AdminUserController(AppDbContext context, IEmailSender emailSender )
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: admin/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // GET: admin/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }

        // POST: admin/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (string.IsNullOrEmpty(request.Password))
                return BadRequest("Password is required.");

            // Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                Age = request.Age,
                Gender = request.Gender,
                HeightCm = request.HeightCm,
                WeightKg = request.WeightKg,
                TargetWeightKg = request.TargetWeightKg,
                Goal = request.Goal,
                FitnessLevel = request.FitnessLevel,
                Availability = request.Availability,
                Equipment = request.Equipment,
                ActivityLevel = request.ActivityLevel,
                Role = "User",  // default role
                IsEmailVerified = false,
                EmailVerificationToken = Guid.NewGuid().ToString()  // generate token here
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Build verification URL for the email
            var verificationUrl = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={user.EmailVerificationToken}";

            // Send verification email - you need an email sender service injected (like _emailSender)
            await _emailSender.SendEmailAsync(user.Email, 
                "Verify your email for FitFuel", 
                $"Welcome to FitFuel – Confirm your email: {verificationUrl}", 
                $"<p>Welcome to FitFuel – Confirm your email by clicking<a href='{verificationUrl}'>here</a>.</p>");

            return CreatedAtAction(nameof(GetUserById), new { id = user.UserId }, new { user.UserId, user.Email, user.Name });
        }

        
        // PUT: admin/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] User updatedUser)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            // Update other profile fields
            user.Name = updatedUser.Name;
            user.HeightCm = updatedUser.HeightCm;
            user.WeightKg = updatedUser.WeightKg;
            user.TargetWeightKg = updatedUser.TargetWeightKg;
            user.Age = updatedUser.Age;
            user.Gender = updatedUser.Gender;
            user.Goal = updatedUser.Goal;
            user.FitnessLevel = updatedUser.FitnessLevel;
            user.Availability = updatedUser.Availability;
            user.Equipment = updatedUser.Equipment;
            user.ActivityLevel = updatedUser.ActivityLevel;

            // Check if email changed (case-insensitive)
            if (!string.Equals(user.Email, updatedUser.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Email = updatedUser.Email;
                user.IsEmailVerified = false;
                user.EmailVerificationToken = Guid.NewGuid().ToString();

                // Build verification URL
                var verificationUrl = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={user.EmailVerificationToken}";

                // Send verification email
                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Verify your new email for FitFuel",
                    $"Please verify your new email by clicking this link: {verificationUrl}",
                    $"<p>Please verify your new email by clicking <a href='{verificationUrl}'>here</a>.</p>"
                );
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "User updated successfully. If you changed your email, please verify it via the email sent." });
        }


        // DELETE: admin/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
