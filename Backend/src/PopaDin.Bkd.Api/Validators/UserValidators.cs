using FluentValidation;
using PopaDin.Bkd.Api.Dtos.User;

namespace PopaDin.Bkd.Api.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Saldo deve ser maior ou igual a zero");
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.Password)
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}

public class AdjustBalanceRequestValidator : AbstractValidator<AdjustBalanceRequest>
{
    public AdjustBalanceRequestValidator()
    {
        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0).WithMessage("Saldo deve ser maior ou igual a zero");
    }
}
