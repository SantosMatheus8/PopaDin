using PopaDin.Bkd.Domain.Enums;
using PopaDin.Bkd.Domain.Exceptions;

namespace PopaDin.Bkd.Domain.Models;

public class Record
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public OperationEnum Operation { get; set; }
    public decimal Value { get; set; }
    public FrequencyEnum Frequency { get; set; }
    public DateTime? ReferenceDate { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Tag> Tags { get; set; } = new List<Tag>();
    public User User { get; set; } = new();
    public string? InstallmentGroupId { get; set; }
    public int? InstallmentIndex { get; set; }
    public int? InstallmentTotal { get; set; }

    public DateTime? RecurrenceEndDate { get; set; }

    public bool IsRecurring => Frequency != FrequencyEnum.OneTime && InstallmentGroupId == null;

    public void ValidateValue()
    {
        if (Value <= 0)
            throw new UnprocessableEntityException("O valor deve ser maior que zero.");
    }

    public decimal CalculateBalanceImpact()
    {
        return Operation == OperationEnum.Deposit ? Value : -Value;
    }
}
