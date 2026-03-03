using System.ComponentModel.DataAnnotations;

namespace PopaDin.Bkd.Api.Dtos.User;

public class CreateUserRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "O email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "A senha é obrigatória")]
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string Password { get; set; } = "";

    public double Balance { get; set; }
}