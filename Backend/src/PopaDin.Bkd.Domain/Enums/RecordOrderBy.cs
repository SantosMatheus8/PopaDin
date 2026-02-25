using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PopaDin.Bkd.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
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

