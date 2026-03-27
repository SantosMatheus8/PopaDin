using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class ListRecordsRequest
{
    public int? Id { get; set; }
    public OperationEnum? Operation { get; set; }
    public FrequencyEnum? Frequency { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public RecordOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;

    // Filtros avançados
    public string? Name { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public List<int>? TagIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}