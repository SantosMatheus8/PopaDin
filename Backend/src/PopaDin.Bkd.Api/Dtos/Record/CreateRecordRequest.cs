using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class CreateRecordRequest
{
    public OperationEnum Operation { get; set; }
    public decimal Value { get; set; }
    public FrequencyEnum Frequency { get; set; }
    public List<int> TagIds { get; set; } = [];
}
