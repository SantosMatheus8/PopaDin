using System.ComponentModel;

namespace PopaDin.Bkd.Domain.Enums;

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

