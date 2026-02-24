using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class UpdateRecordRequest
{
    public OperationEnum Operation { get; set; }
    public double Value { get; set; }
    public int Frequency { get; set; }
    // public List<RecordTagModel> RecordTags { get; set; } = new List<RecordTagModel>();
}