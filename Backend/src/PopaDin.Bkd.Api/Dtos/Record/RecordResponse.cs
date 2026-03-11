using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class RecordResponse
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public OperationEnum Operation { get; set; }
    public decimal Value { get; set; }
    public FrequencyEnum Frequency { get; set; }
    public int UserId { get; set; }
    public DateTime ReferenceDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<RecordTagResponse> Tags { get; set; } = new List<RecordTagResponse>();
    public string? InstallmentGroupId { get; set; }
    public int? InstallmentIndex { get; set; }
    public int? InstallmentTotal { get; set; }
}
