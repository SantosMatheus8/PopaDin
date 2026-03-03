using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class RecordResponse
{
    public int Id { get; set; }
    public OperationEnum Operation { get; set; }
    public double Value { get; set; }
    public FrequencyEnum Frequency { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RecordTagResponse> Tags { get; set; } = new List<RecordTagResponse>();
}