using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Tag;

public class TagResponse
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public OperationEnum? TagType { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedAt { get; set; }
}