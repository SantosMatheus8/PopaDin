using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models.Tag;

public class ListTags
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public OperationEnum? TagType { get; set; }
    public string? Description { get; set; }
    public int UserId { get; set; }
    public OrderDirection OrderDirection { get; set; }
    public TagOrderBy OrderBy { get; set; }
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 20;
}
