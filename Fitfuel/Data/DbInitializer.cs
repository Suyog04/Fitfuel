using FitFuel.Models;
using BCrypt.Net;

namespace FitFuel.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; // DB already seeded
            }

            var users = new User[]
            {
                new User
                {
                    UserId = Guid.NewGuid(),
                    Name = "Test User",
                    Email = "test@example.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}