using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PopaDin.Bkd.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserOrderBy
{
    [Description("u.Id")]
    Id,
    [Description("u.Name")]
    Name,
    [Description("u.Goal")]
    Email,
    [Description("u.CurrentAmount")]
    Balance
}

