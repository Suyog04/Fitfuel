using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FitFuel.Data;
using FitFuel.Models;
using FitFuel.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http; // For CookieOptions

namespace FitFuel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context,
            ILogger<AuthController> logger,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
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
                    IsEmailVerified = false,
                    EmailVerificationToken = Guid.NewGuid().ToString(),
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

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

                _logger.LogInformation("Verification email send completed for {Email}", user.Email);

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
            user.EmailVerificationToken = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var html = @"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <title>Email Verified | FitFuel</title>
  <style>
    body {
      background-color: #f0f4f8;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      margin: 0;
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100vh;
    }
    .card {
      background-color: #fff;
      border-radius: 12px;
      padding: 40px;
      text-align: center;
      box-shadow: 0 4px 20px rgba(0,0,0,0.1);
      max-width: 400px;
      animation: fadeIn 0.6s ease-in-out;
    }
    .checkmark {
      font-size: 60px;
      color: #2ecc71;
      animation: popIn 0.3s ease-out;
    }
    h2 {
      color: #2d3436;
      margin-top: 20px;
    }
    p {
      color: #636e72;
      margin: 15px 0;
    }
    .btn {
      display: inline-block;
      margin-top: 25px;
      padding: 12px 24px;
      background-color: #22a6b3;
      color: white;
      text-decoration: none;
      border-radius: 8px;
      font-weight: bold;
      transition: background-color 0.2s;
    }
    .btn:hover {
      background-color: #1e90a0;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(20px); }
      to { opacity: 1; transform: translateY(0); }
    }
    @keyframes popIn {
      0% { transform: scale(0); opacity: 0; }
      100% { transform: scale(1); opacity: 1; }
    }
  </style>
</head>
<body>
  <div class='card'>
    <div class='checkmark'>✔️</div>
    <h2>Email Verified!</h2>
    <p>Your email has been successfully verified. You can now log in to your FitFuel account.</p>
    <a href='/login' class='btn'>Go to Login</a>
  </div>
</body>
</html>";

            return Content(html, "text/html");
        }

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

            var token = GenerateJwtToken(user);

            // Issue JWT as secure HttpOnly cookie for Admins only
            if (user.Role == "Admin")
            {
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // Set to true in production (HTTPS)
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(12)
                });

                return Ok(new
                {
                    Message = "Admin login successful",
                    UserId = user.UserId,
                    Name = user.Name,
                    Email = user.Email
                });
            }

            // For other users, return JWT token in response body
            return Ok(new
            {
                Message = "Login successful",
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Token = token
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
                var toEmail = "Fitfuel.befit@protonmail.com";
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

        

        // Helper methods

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
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
