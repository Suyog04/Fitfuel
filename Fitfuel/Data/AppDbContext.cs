using FitFuel.Models;
using Microsoft.EntityFrameworkCore;

namespace FitFuel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CalorieEntry> CalorieEntries { get; set; }
        public DbSet<StepEntry> StepEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            // Configure CalorieEntry entity
            modelBuilder.Entity<CalorieEntry>()
                .HasKey(e => e.EntryId);

            modelBuilder.Entity<CalorieEntry>()
                .HasOne(e => e.User)
                .WithMany(u => u.CalorieEntries)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CalorieEntry>()
                .Property(e => e.Meal)
                .HasConversion<string>();

            // Configure StepEntry entity
            modelBuilder.Entity<StepEntry>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<StepEntry>()
                .HasOne(e => e.User)
                .WithMany() // No navigation list in User model for steps (optional)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}