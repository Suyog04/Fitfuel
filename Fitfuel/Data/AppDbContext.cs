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
        public DbSet<PredictedCalorie> PredictedCalories { get; set; }  

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

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

            modelBuilder.Entity<StepEntry>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<StepEntry>()
                .HasOne(e => e.User)
                .WithMany() // No navigation property for StepEntries on User
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Enforce storing Date as date-only (no time component) in PostgreSQL
            modelBuilder.Entity<StepEntry>()
                .Property(e => e.Date)
                .HasColumnType("date");

            // PredictedCalorie entity configuration
            modelBuilder.Entity<PredictedCalorie>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<PredictedCalorie>()
                .HasOne(p => p.User)
                .WithMany() 
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PredictedCalorie>()
                .Property(p => p.Date)
                .HasColumnType("date");
        }
    }
}
