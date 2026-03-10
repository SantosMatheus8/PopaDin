using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Api.Dtos.Tag;

public class CreateTagRequest
{
    public string Name { get; set; } = "";
    public OperationEnum? TagType { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
}