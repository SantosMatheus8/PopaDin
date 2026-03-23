using FluentValidation;
using PopaDin.Bkd.Api.Dtos.Alert;

namespace PopaDin.Bkd.Api.Validators;

public class CreateAlertRequestValidator : AbstractValidator<CreateAlertRequest>
{
    public CreateAlertRequestValidator()
    {
        RuleFor(x => x.Threshold)
            .GreaterThan(0).WithMessage("O valor limite deve ser maior que zero");
    }
}
