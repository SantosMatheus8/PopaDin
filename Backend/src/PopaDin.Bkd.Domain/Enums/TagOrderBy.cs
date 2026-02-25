using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PopaDin.Bkd.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
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

