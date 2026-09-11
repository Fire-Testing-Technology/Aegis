using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Aegis.Server.AspNetCore.Utilities;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field?.GetCustomAttribute<DisplayAttribute>() is { } display)
        {
            var name = display.GetName();
            if (!string.IsNullOrWhiteSpace(name))
                return name;
        }

        return value.ToString();
    }

    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field?.GetCustomAttribute<DescriptionAttribute>() is { } attr
            && !string.IsNullOrWhiteSpace(attr.Description))
        {
            return attr.Description;
        }

        return string.Empty;
    }
}
