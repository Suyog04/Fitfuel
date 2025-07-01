using System.ComponentModel.DataAnnotations;

namespace FitFuel.Models.DTOs
{
    public class ResetPasswordRequest
    {
        [Required] public string Token { get; set; }

        [Required, StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}