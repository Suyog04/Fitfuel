using System.ComponentModel.DataAnnotations;

namespace FitFuel.Models.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress] public string Email { get; set; }
    }
}