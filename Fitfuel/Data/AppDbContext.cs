// Data/AppDbContext.cs
using FitFuel.Models;
using Microsoft.EntityFrameworkCore;

namespace FitFuel.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<CalorieEntry> CalorieEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure primary keys
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);
                
            modelBuilder.Entity<CalorieEntry>()
                .HasKey(e => e.EntryId);
            
            // Configure relationships
            modelBuilder.Entity<CalorieEntry>()
                .HasOne(e => e.User)
                .WithMany(u => u.CalorieEntries)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure enum conversion to store as string
            modelBuilder.Entity<CalorieEntry>()
                .Property(e => e.Meal)
                .HasConversion<string>();
        }
    }
}