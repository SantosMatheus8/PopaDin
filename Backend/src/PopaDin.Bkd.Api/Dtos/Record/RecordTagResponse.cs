using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Record;

public class RecordTagResponse
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public OperationEnum? TagType { get; set; }
    public string? Color { get; set; }
}
