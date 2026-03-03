using PopaDin.Bkd.Domain.Enums;

namespace PopaDin.Bkd.Domain.Models.Tag;

public class Tag
{
    public int? Id { get; set; }
    public string Name { get; set; } = "";
    public OperationEnum? TagType { get; set; }
    public string? Description { get; set; }
    public User.User User { get; set; } = new();
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
