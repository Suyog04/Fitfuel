using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitFuel.Data;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using BCrypt.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.Data;
using FitFuel.Models.DTOs;
using Microsoft.Extensions.Logging;
using ForgotPasswordRequest = FitFuel.Models.DTOs.ForgotPasswordRequest;
using LoginRequest = FitFuel.Models.DTOs.LoginRequest;
using RegisterRequest = FitFuel.Models.DTOs.RegisterRequest;
using ResetPasswordRequest = FitFuel.Models.DTOs.ResetPasswordRequest;

namespace FitFuel.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IEmailSender _emailSender;

    public AuthController(AppDbContext context, ILogger<AuthController> logger, IEmailSender emailSender)
    {
        _context = context;
        _logger = logger;
        _emailSender = emailSender;
    }

    [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    try
    {
        if (!IsPasswordStrong(request.Password))
        {
            return BadRequest("Password must be at least 8 characters with uppercase, lowercase, number, and special character");
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
        {
            return Conflict("A user with this email already exists.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, BCrypt.Net.BCrypt.GenerateSalt(12));

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            HeightCm = request.HeightCm,
            WeightKg = request.WeightKg,
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc),
            IsEmailVerified = false,
            EmailVerificationToken = Guid.NewGuid().ToString()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 🔍 Log before sending email
        _logger.LogInformation("📤 Attempting to send verification email to {Email}", user.Email);

        var verificationUrl = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={user.EmailVerificationToken}";
        var subject = "Verify your email for FitFuel";
        var plainTextContent = $"Please verify your email by clicking this link: {verificationUrl}";
        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8' />
  <title>Verify Your Email</title>
  <style>
    body {{
      font-family: Arial, sans-serif;
      background-color: #f4f4f7;
      color: #51545e;
      margin: 0;
      padding: 0;
    }}
    .container {{
      max-width: 600px;
      margin: 40px auto;
      background-color: #ffffff;
      padding: 20px;
      border-radius: 8px;
      box-shadow: 0 2px 5px rgba(0,0,0,0.1);
    }}
    .header {{
      text-align: center;
      padding-bottom: 20px;
      border-bottom: 1px solid #eaeaea;
    }}
    h1 {{
      color: #333333;
    }}
    .button {{
      display: inline-block;
      padding: 12px 24px;
      margin: 30px 0;
      background-color: #22a6b3;
      color: white !important;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
    }}
    .footer {{
      font-size: 12px;
      color: #999999;
      text-align: center;
      margin-top: 40px;
      border-top: 1px solid #eaeaea;
      padding-top: 10px;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>Welcome to FitFuel!</h1>
    </div>
    <p>Hi {user.Name},</p>
    <p>Thank you for registering with FitFuel. Please verify your email address by clicking the button below:</p>
    <p style='text-align:center;'>
      <a href='{verificationUrl}' class='button'>Verify Email</a>
    </p>
    <p>If the button above doesn’t work, copy and paste the following URL into your browser:</p>
    <p><a href='{verificationUrl}'>{verificationUrl}</a></p>
    <p>Cheers,<br/>The FitFuel Team</p>
    <div class='footer'>
      <p>If you did not sign up for this account, you can safely ignore this email.</p>
    </div>
  </div>
</body>
</html>";


        await _emailSender.SendEmailAsync(user.Email, subject, plainTextContent, htmlContent);


        // ✅ Log after email send
        _logger.LogInformation("✅ Verification email send completed for {Email}", user.Email);

        return Ok(new
        {
            Message = "User registered successfully. Please verify your email before logging in.",
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email
        });
    }
    catch (Exception ex)
    {
        //  Log exception if email fails
        _logger.LogError(ex, " Registration error for {Email}", request.Email);
        return StatusCode(500, "Internal server error");
    }
}


    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return BadRequest("Invalid token");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        if (user == null)
            return NotFound("Invalid token or user not found");

        if (user.IsEmailVerified)
            return BadRequest("Email is already verified");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null; // Optional: clear token after verification

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok("Email successfully verified. You can now log in.");
    }

    // Login method, etc. (make sure to check IsEmailVerified on login)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized("Invalid email or password");

        if (!user.IsEmailVerified)
            return Unauthorized("Email not verified. Please verify your email before logging in.");

        if (string.IsNullOrEmpty(user.PasswordHash))
            return StatusCode(500, "Password hash not set for this user.");

        if (!IsValidBCryptHash(user.PasswordHash))
            return StatusCode(500, "Invalid password hash format");

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
            return Unauthorized("Invalid email or password");

        return Ok(new
        {
            Message = "Login successful",
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Token = GenerateJwtToken(user) // optional
        });
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return Ok("If this email is registered, a reset link has been sent.");

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        var resetLink = $"{Request.Scheme}://{Request.Host}/api/auth/reset-password?token={user.PasswordResetToken}";
        await _emailSender.SendEmailAsync(user.Email, "Reset your FitFuel password",
            $"Reset your password using this link: {resetLink}",
            $"<p>Click <a href='{resetLink}'>here</a> to reset your password. This link expires in 1 hour.</p>");

        return Ok("If this email is registered, a reset link has been sent.");
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == request.Token &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return BadRequest("Invalid or expired token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _context.SaveChangesAsync();

        return Ok("Password has been reset successfully.");
    }
    
    
    [HttpGet("test-send-email")]
    public async Task<IActionResult> TestSendEmail()
    {
        try
        {
            var toEmail = "bistasuyog33@gmail.com"; // your email here
            var subject = "Test Email from FitFuel";
            var plainTextContent = "This is a test email sent to check SendGrid integration.";
            var htmlContent = "<p>This is a <strong>test email</strong> sent to check SendGrid integration.</p>";

            await _emailSender.SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
            return Ok("Test email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email");
            return StatusCode(500, "Failed to send test email");
        }
    }

    
    // Helper methods as before...

    private bool IsValidBCryptHash(string hash)
    {
        return Regex.IsMatch(hash, @"^\$2[abxy]\$\d{2}\$.{53}$");
    }

    private bool IsPasswordStrong(string password)
    {
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
    }

    private string GenerateJwtToken(User user)
    {
        return $"JWT-TOKEN-FOR-{user.UserId}";
    }
}


