using FluentValidation;
using PopaDin.Bkd.Api.Dtos.Budget;

namespace PopaDin.Bkd.Api.Validators;

public class CreateBudgetRequestValidator : AbstractValidator<CreateBudgetRequest>
{
    public CreateBudgetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.Goal)
            .GreaterThan(0).WithMessage("A meta deve ser maior que zero");
    }
}

public class UpdateBudgetRequestValidator : AbstractValidator<UpdateBudgetRequest>
{
    public UpdateBudgetRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.Goal)
            .GreaterThan(0).WithMessage("A meta deve ser maior que zero");
    }
}
