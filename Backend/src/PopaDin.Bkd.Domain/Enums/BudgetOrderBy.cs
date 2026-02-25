using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PopaDin.Bkd.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BudgetOrderBy
{
    [Description("b.Id")]
    Id,
    [Description("b.Name")]
    Name,
    [Description("b.Goal")]
    Goal,
    [Description("b.CurrentAmount")]
    CurrentAmount,
    [Description("b.FinishAt")]
    FinishAt,
}

