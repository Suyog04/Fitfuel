using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitFuel.Models;

public class CalorieEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid EntryId { get; set; }

    [Required]
    [ForeignKey("User")]
    public Guid UserId { get; set; }
        
    public virtual User User { get; set; }

    [Required]
    [StringLength(200)]
    public string FoodItem { get; set; }
        
    [Required]
    [Range(1, 5000)]
    public double WeightInGrams { get; set; }

    [Required]
    public MealType Meal { get; set; } // Reference to the enum

    [DataType(DataType.DateTime)]
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public double Calories { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public double Protein { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public double Carbs { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public double Fats { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public double Fiber { get; set; }
}

public enum MealType
{
    Breakfast,
    MorningSnack,
    Lunch,
    AfternoonSnack,
    Dinner,
    EveningSnack
}
