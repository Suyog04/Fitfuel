using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using FitFuel.Models;
using System.ComponentModel.DataAnnotations;
using BCrypt.Net; // Correct namespace

namespace FitFuel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return StatusCode(500, "Password hash not set for this user.");
        }

        // Use BCrypt.Net.BCrypt directly
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return Unauthorized("Invalid email or password");
        }

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email
        });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return Conflict("A user with this email already exists.");
        }

        // Use BCrypt.Net.BCrypt directly
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

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
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}