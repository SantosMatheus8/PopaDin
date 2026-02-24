using System.ComponentModel;

namespace PopaDin.Bkd.Domain.Enums;

public enum RecordOrderBy
{
    [Description("r.Id")]
    Id,
    [Description("r.CreatedAt")]
    CreatedAt,
    [Description("r.Frequency")]
    Frequency,
    [Description("r.Value")]
    Value,
    [Description("r.Operation")]
    Operation,
}

