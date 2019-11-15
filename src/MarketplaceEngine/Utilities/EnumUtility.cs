// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel;
using System.Reflection;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Utility for working with enums - conversion, description extraction, etc.
/// </summary>
public static class EnumUtility
{
    /// <summary>
    /// Gets the description of an enum value from [Description] attribute.
    /// </summary>
    public static string GetDescription<T>(T value) where T : Enum
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Converts a string to an enum value safely.
    /// Returns default if conversion fails.
    /// </summary>
    public static T? TryParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (Enum.TryParse<T>(value, ignoreCase: true, out var result))
            return result;

        return null;
    }

    /// <summary>
    /// Gets all values of an enum type.
    /// </summary>
    public static List<T> GetEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    /// <summary>
    /// Gets all names of an enum type.
    /// </summary>
    public static List<string> GetEnumNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T)).ToList();
    }

    /// <summary>
    /// Converts enum to dictionary with name-value pairs.
    /// </summary>
    public static Dictionary<string, int> GetEnumDictionary<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .ToDictionary(e => e.ToString(), e => Convert.ToInt32(e));
    }

    /// <summary>
    /// Checks if an enum value has a specific flag.
    /// </summary>
    public static bool HasFlag<T>(T value, T flag) where T : Enum
    {
        return value.HasFlag(flag);
    }
}
