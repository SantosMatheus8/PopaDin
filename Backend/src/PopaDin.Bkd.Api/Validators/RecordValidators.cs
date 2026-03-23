using FluentValidation;
using PopaDin.Bkd.Api.Dtos.Record;

namespace PopaDin.Bkd.Api.Validators;

public class CreateRecordRequestValidator : AbstractValidator<CreateRecordRequest>
{
    public CreateRecordRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("O valor deve ser maior que zero");

        RuleFor(x => x.Installments)
            .InclusiveBetween(2, 48).WithMessage("O número de parcelas deve ser entre 2 e 48")
            .When(x => x.Installments.HasValue);
    }
}

public class UpdateRecordRequestValidator : AbstractValidator<UpdateRecordRequest>
{
    public UpdateRecordRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("O valor deve ser maior que zero");

        RuleFor(x => x.Installments)
            .InclusiveBetween(2, 48).WithMessage("O número de parcelas deve ser entre 2 e 48")
            .When(x => x.Installments.HasValue);
    }
}

public class ExportRecordsRequestValidator : AbstractValidator<ExportRecordsRequest>
{
    public ExportRecordsRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("A data de início é obrigatória");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("A data de fim é obrigatória")
            .GreaterThan(x => x.StartDate).WithMessage("A data de fim deve ser posterior à data de início");
    }
}
