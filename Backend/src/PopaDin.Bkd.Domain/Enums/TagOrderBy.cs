using System.ComponentModel;

namespace PopaDin.Bkd.Domain.Enums;

public enum TagOrderBy
{
    [Description("t.Id")]
    Id,
    [Description("t.Name")]
    Name,
    [Description("t.TagType")]
    TagType,
    [Description("t.CreatedAt")]
    CreatedAt,
}

