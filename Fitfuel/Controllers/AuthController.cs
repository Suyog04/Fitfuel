using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using FitFuel.Models;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace FitFuel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, ILogger<AuthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning($"Login attempt for unknown email: {request.Email}");
                return Unauthorized("Invalid email or password");
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogError($"User {user.UserId} has no password hash");
                return StatusCode(500, "Password hash not set for this user.");
            }

            // Validate BCrypt hash format before verification
            if (!IsValidBCryptHash(user.PasswordHash))
            {
                _logger.LogError($"Invalid BCrypt hash format for user {user.UserId}");
                return StatusCode(500, "Invalid password hash format");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                _logger.LogWarning($"Invalid password for user: {user.Email}");
                return Unauthorized("Invalid email or password");
            }

            return Ok(new
            {
                Message = "Login successful",
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Token = GenerateJwtToken(user) // Implement JWT generation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for {Email}", request.Email);
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Validate password strength
            if (!IsPasswordStrong(request.Password))
            {
                return BadRequest("Password must be at least 8 characters with uppercase, lowercase, number and special character");
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                return Conflict("A user with this email already exists.");
            }

            // Enhanced hashing with work factor
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(
                request.Password, 
                BCrypt.Net.BCrypt.GenerateSalt(12) // Adjust work factor as needed
            );

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "User registered successfully",
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration error for {Email}", request.Email);
            return StatusCode(500, "Internal server error");
        }
    }

    private bool IsValidBCryptHash(string hash)
    {
        // BCrypt hash should start with $2a$, $2b$, $2x$ or $2y$
        return Regex.IsMatch(hash, @"^\$2[abxy]\$\d{2}\$.{53}$");
    }

    private bool IsPasswordStrong(string password)
    {
        // Minimum 8 characters, at least one uppercase, one lowercase, one number and one special character
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
    }

    private string GenerateJwtToken(User user)
    {
        // Implement actual JWT token generation using System.IdentityModel.Tokens.Jwt
        // This is a placeholder implementation
        return $"JWT-TOKEN-FOR-{user.UserId}";
    }
}

public class LoginRequest
{
    [Required, EmailAddress] 
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}