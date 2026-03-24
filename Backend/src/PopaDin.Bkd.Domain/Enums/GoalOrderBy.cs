using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PopaDin.Bkd.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GoalOrderBy
{
    [Description("g.Id")]
    Id,
    [Description("g.Name")]
    Name,
    [Description("g.TargetAmount")]
    TargetAmount,
    [Description("g.Deadline")]
    Deadline,
    [Description("g.FinishAt")]
    FinishAt,
}
