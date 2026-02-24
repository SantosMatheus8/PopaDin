using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class ListRecordsRequest
{
    public int? Id { get; set; }
    public OperationEnum? Operation { get; set; }
    public int? Frequency { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public RecordOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}