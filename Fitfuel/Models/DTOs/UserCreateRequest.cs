using System.ComponentModel.DataAnnotations;

namespace Fitfuel.Models.DTOs;

public class UserCreateRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required, StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}
