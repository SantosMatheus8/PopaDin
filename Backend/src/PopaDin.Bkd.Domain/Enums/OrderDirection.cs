using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PopaDin.Bkd.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderDirection
{
    [Description("ASC")]
    ASC,
    [Description("DESC")]
    DESC
}

