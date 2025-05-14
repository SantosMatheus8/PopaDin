using System.ComponentModel;

namespace PopaDin.Bkd.Domain.Enums;

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

