using System.ComponentModel.DataAnnotations;

namespace PopaDin.Bkd.Api.Dtos.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "A senha é obrigatória")]
    public string Password { get; set; } = "";
}
