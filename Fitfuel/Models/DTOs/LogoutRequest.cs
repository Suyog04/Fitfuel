using System;
using System.ComponentModel.DataAnnotations;

namespace FitFuel.Models.DTOs
{
    public class LogoutRequest
    {
        [Required]
        public Guid UserId { get; set; }
    }
}