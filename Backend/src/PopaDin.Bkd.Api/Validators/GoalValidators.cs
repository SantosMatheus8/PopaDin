using FluentValidation;
using PopaDin.Bkd.Api.Dtos.Goal;

namespace PopaDin.Bkd.Api.Validators;

public class CreateGoalRequestValidator : AbstractValidator<CreateGoalRequest>
{
    public CreateGoalRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("O valor da meta deve ser maior que zero");
    }
}

public class UpdateGoalRequestValidator : AbstractValidator<UpdateGoalRequest>
{
    public UpdateGoalRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("O valor da meta deve ser maior que zero");
    }
}
