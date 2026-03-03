using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models.Record;

public class ListRecords
{
    public int? Id { get; set; }
    public double? Value { get; set; }
    public FrequencyEnum? Frequency { get; set; }
    public OperationEnum? Operation { get; set; }
    public int UserId { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public RecordOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}
