using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitFuel.Models
{
    public class UserLog
    {
        [Key]
        public Guid LogId { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }

        public virtual User User { get; set; }
    }
}