using System.ComponentModel;

namespace PopaDin.Bkd.Domain.Utils;

public static class EnumExtensions
{
    public static string GetEnumDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = (DescriptionAttribute)Attribute.GetCustomAttribute(field!, typeof(DescriptionAttribute))!;
        return attribute == null! ? value.ToString() : attribute.Description;
    }
}

