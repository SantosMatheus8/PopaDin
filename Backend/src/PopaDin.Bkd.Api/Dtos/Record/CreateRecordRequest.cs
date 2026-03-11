using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class CreateRecordRequest
{
    public string Name { get; set; } = "";
    public OperationEnum Operation { get; set; }
    public decimal Value { get; set; }
    public FrequencyEnum Frequency { get; set; }
    public List<int> TagIds { get; set; } = [];
    public DateTime? ReferenceDate { get; set; }
    public int? Installments { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }
}
