#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel;
using System.Reflection;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Provides validation helpers for <see cref="EnumUtility"/> class.
/// Validates that the EnumUtility class has all expected methods with correct signatures.
/// </summary>
public static class EnumUtilityValidation
{
    /// <summary>
    /// Validates the EnumUtility class and its methods for common issues.
    /// </summary>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate that all expected methods exist with correct signatures

        // Validate GetDescription method
        var getDescriptionMethod = typeof(EnumUtility).GetMethod("GetDescription", BindingFlags.Public | BindingFlags.Static);
        if (getDescriptionMethod == null)
        {
            problems.Add("Missing GetDescription method");
        }
        else if (getDescriptionMethod.GetParameters().Length != 1)
        {
            problems.Add("GetDescription method has incorrect parameter count");
        }
        else if (getDescriptionMethod.ReturnType != typeof(string))
        {
            problems.Add("GetDescription method has incorrect return type");
        }

        // Validate TryParseEnum method
        var tryParseEnumMethod = typeof(EnumUtility).GetMethod("TryParseEnum", BindingFlags.Public | BindingFlags.Static);
        if (tryParseEnumMethod == null)
        {
            problems.Add("Missing TryParseEnum method");
        }
        else if (tryParseEnumMethod.GetParameters().Length != 1)
        {
            problems.Add("TryParseEnum method has incorrect parameter count");
        }
        else if (tryParseEnumMethod.ReturnType != typeof(void).MakeByRefType())
        {
            problems.Add("TryParseEnum method has incorrect return type");
        }

        // Validate GetEnumValues method
        var getEnumValuesMethod = typeof(EnumUtility).GetMethod("GetEnumValues", BindingFlags.Public | BindingFlags.Static);
        if (getEnumValuesMethod == null)
        {
            problems.Add("Missing GetEnumValues method");
        }
        else if (getEnumValuesMethod.GetParameters().Length != 0)
        {
            problems.Add("GetEnumValues method has incorrect parameter count");
        }
        else if (!getEnumValuesMethod.ReturnType.IsGenericType || getEnumValuesMethod.ReturnType.GetGenericTypeDefinition() != typeof(List<>))
        {
            problems.Add("GetEnumValues method has incorrect return type");
        }

        // Validate GetEnumNames method
        var getEnumNamesMethod = typeof(EnumUtility).GetMethod("GetEnumNames", BindingFlags.Public | BindingFlags.Static);
        if (getEnumNamesMethod == null)
        {
            problems.Add("Missing GetEnumNames method");
        }
        else if (getEnumNamesMethod.GetParameters().Length != 0)
        {
            problems.Add("GetEnumNames method has incorrect parameter count");
        }
        else if (!getEnumNamesMethod.ReturnType.IsGenericType || getEnumNamesMethod.ReturnType.GetGenericTypeDefinition() != typeof(List<>))
        {
            problems.Add("GetEnumNames method has incorrect return type");
        }

        // Validate GetEnumDictionary method
        var getEnumDictionaryMethod = typeof(EnumUtility).GetMethod("GetEnumDictionary", BindingFlags.Public | BindingFlags.Static);
        if (getEnumDictionaryMethod == null)
        {
            problems.Add("Missing GetEnumDictionary method");
        }
        else if (getEnumDictionaryMethod.GetParameters().Length != 0)
        {
            problems.Add("GetEnumDictionary method has incorrect parameter count");
        }
        else if (!getEnumDictionaryMethod.ReturnType.IsGenericType || getEnumDictionaryMethod.ReturnType.GetGenericTypeDefinition() != typeof(Dictionary<,>))
        {
            problems.Add("GetEnumDictionary method has incorrect return type");
        }

        // Validate HasFlag method
        var hasFlagMethod = typeof(EnumUtility).GetMethod("HasFlag", BindingFlags.Public | BindingFlags.Static);
        if (hasFlagMethod == null)
        {
            problems.Add("Missing HasFlag method");
        }
        else if (hasFlagMethod.GetParameters().Length != 2)
        {
            problems.Add("HasFlag method has incorrect parameter count");
        }
        else if (hasFlagMethod.ReturnType != typeof(bool))
        {
            problems.Add("HasFlag method has incorrect return type");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the EnumUtility class is valid (has no validation problems).
    /// </summary>
    /// <returns>True if valid; false if any validation problems exist.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the EnumUtility class is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"EnumUtility validation failed with {problems.Count} problem(s):{Environment.NewLine}" +
            string.Join($"{Environment.NewLine}", problems));
    }
}