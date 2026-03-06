using System.ComponentModel.DataAnnotations;

namespace PopaDin.Bkd.Api.Dtos.User;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    public string Name { get; set; } = "";

    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
    public string? Password { get; set; }

    public decimal Balance { get; set; }
}
