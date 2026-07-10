#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Validation helpers for StringUtility that check for common issues in string operations.
/// </summary>
public static class StringUtilityValidation
{
    /// <summary>
    /// Validates the StringUtility class and its methods for common issues.
    /// </summary>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate that all expected methods exist with correct signatures

        // Validate Truncate method
        if (typeof(StringUtility).GetMethod("Truncate") == null)
        {
            problems.Add("Missing Truncate method");
        }

        // Validate ToTitleCase method
        if (typeof(StringUtility).GetMethod("ToTitleCase") == null)
        {
            problems.Add("Missing ToTitleCase method");
        }

        // Validate ToSlug method
        if (typeof(StringUtility).GetMethod("ToSlug") == null)
        {
            problems.Add("Missing ToSlug method");
        }

        // Validate Repeat method
        if (typeof(StringUtility).GetMethod("Repeat") == null)
        {
            problems.Add("Missing Repeat method");
        }

        // Validate ContainsAny method
        if (typeof(StringUtility).GetMethod("ContainsAny") == null)
        {
            problems.Add("Missing ContainsAny method");
        }

        // Validate MaskEmail method
        if (typeof(StringUtility).GetMethod("MaskEmail") == null)
        {
            problems.Add("Missing MaskEmail method");
        }

        // Validate MaskPhoneNumber method
        if (typeof(StringUtility).GetMethod("MaskPhoneNumber") == null)
        {
            problems.Add("Missing MaskPhoneNumber method");
        }

        // Validate RemoveSpecialCharacters method
        if (typeof(StringUtility).GetMethod("RemoveSpecialCharacters") == null)
        {
            problems.Add("Missing RemoveSpecialCharacters method");
        }

        // Validate GenerateRandomString method
        if (typeof(StringUtility).GetMethod("GenerateRandomString") == null)
        {
            problems.Add("Missing GenerateRandomString method");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the StringUtility class is valid (has no validation problems).
    /// </summary>
    /// <returns>True if valid; false if any validation problems exist.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures that the StringUtility class is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"StringUtility validation failed with {problems.Count} problem(s):{Environment.NewLine}" +
            string.Join($"{Environment.NewLine}", problems));
    }
}
