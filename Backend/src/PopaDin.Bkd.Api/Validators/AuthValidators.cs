using FluentValidation;
using PopaDin.Bkd.Api.Dtos.Auth;

namespace PopaDin.Bkd.Api.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória");
    }
}
