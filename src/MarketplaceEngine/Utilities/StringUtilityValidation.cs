#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =========================================================================

using System.Globalization;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Validation helpers for StringUtility that verify the correctness of string operations.
/// </summary>
public static class StringUtilityValidation
{
    /// <summary>
    /// Validates the StringUtility class methods for common issues and edge cases.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use for culture-sensitive operations. Defaults to InvariantCulture.</param>
    /// <returns>An enumerable of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if cultureInfo is null.</exception>
    public static IReadOnlyList<string> Validate(CultureInfo? cultureInfo = null)
    {
        ArgumentNullException.ThrowIfNull(cultureInfo);

        var problems = new List<string>();
        var culture = cultureInfo ?? CultureInfo.InvariantCulture;

        // Validate Truncate method
        try
        {
            var result = StringUtility.Truncate(null, 10);
            if (result != string.Empty)
            {
                problems.Add("Truncate: Null input should return empty string");
            }

            result = StringUtility.Truncate("hello", 10);
            if (result != "hello")
            {
                problems.Add("Truncate: Should not truncate strings shorter than maxLength");
            }

            result = StringUtility.Truncate("hello world", 5);
            if (result != "hello")
            {
                problems.Add("Truncate: Should truncate to maxLength without suffix");
            }

            result = StringUtility.Truncate("hello world", 5, "...");
            if (result != "he...")
            {
                problems.Add("Truncate: Should account for suffix length in truncation");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Truncate: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ToTitleCase method
        try
        {
            var result = StringUtility.ToTitleCase(null);
            if (result != string.Empty)
            {
                problems.Add("ToTitleCase: Null input should return empty string");
            }

            result = StringUtility.ToTitleCase("");
            if (result != string.Empty)
            {
                problems.Add("ToTitleCase: Empty input should return empty string");
            }

            result = StringUtility.ToTitleCase("hello world");
            if (result != "Hello World")
            {
                problems.Add("ToTitleCase: Should capitalize first letter of each word");
            }

            result = StringUtility.ToTitleCase("hElLo WoRlD");
            if (result != "Hello World")
            {
                problems.Add("ToTitleCase: Should handle mixed case input correctly");
            }

            result = StringUtility.ToTitleCase("  multiple   spaces  ");
            if (result != "Multiple   Spaces")
            {
                problems.Add("ToTitleCase: Should preserve multiple spaces between words");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToTitleCase: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ToSlug method
        try
        {
            var result = StringUtility.ToSlug(null);
            if (result != string.Empty)
            {
                problems.Add("ToSlug: Null input should return empty string");
            }

            result = StringUtility.ToSlug("");
            if (result != string.Empty)
            {
                problems.Add("ToSlug: Empty input should return empty string");
            }

            result = StringUtility.ToSlug("Hello World!");
            if (result != "hello-world")
            {
                problems.Add("ToSlug: Should convert to lowercase and replace spaces with hyphens");
            }

            result = StringUtility.ToSlug("Test@Email.com");
            if (result != "testemailcom")
            {
                problems.Add("ToSlug: Should remove special characters");
            }

            result = StringUtility.ToSlug("  trim  spaces  ");
            if (result != "trim-spaces")
            {
                problems.Add("ToSlug: Should trim leading/trailing hyphens and handle multiple spaces");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToSlug: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate Repeat method
        try
        {
            var result = StringUtility.Repeat("abc", 3);
            if (result != "abcabcabc")
            {
                problems.Add("Repeat: Should repeat string the specified number of times");
            }

            result = StringUtility.Repeat("x", 0);
            if (result != string.Empty)
            {
                problems.Add("Repeat: Zero count should return empty string");
            }

            try
            {
                StringUtility.Repeat("abc", -1);
                problems.Add("Repeat: Should throw for negative count");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected behavior
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Repeat: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate ContainsAny method
        try
        {
            var result = StringUtility.ContainsAny("hello world", "test", "world");
            if (!result)
            {
                problems.Add("ContainsAny: Should find substring in text");
            }

            result = StringUtility.ContainsAny("hello world", "test", "foo");
            if (result)
            {
                problems.Add("ContainsAny: Should not find non-existent substring");
            }

            result = StringUtility.ContainsAny("", "test");
            if (result)
            {
                problems.Add("ContainsAny: Empty text should return false");
            }

            result = StringUtility.ContainsAny(null, "test");
            if (result)
            {
                problems.Add("ContainsAny: Null text should return false");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ContainsAny: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate MaskEmail method
        try
        {
            var result = StringUtility.MaskEmail(null);
            if (result != string.Empty)
            {
                problems.Add("MaskEmail: Null input should return empty string");
            }

            result = StringUtility.MaskEmail("");
            if (result != string.Empty)
            {
                problems.Add("MaskEmail: Empty input should return empty string");
            }

            result = StringUtility.MaskEmail("a@domain.com");
            if (result != "*@domain.com")
            {
                problems.Add("MaskEmail: Short local part should be fully masked");
            }

            result = StringUtility.MaskEmail("test@domain.com");
            if (result != "te****@domain.com")
            {
                problems.Add("MaskEmail: Should mask most of local part with visible prefix");
            }

            result = StringUtility.MaskEmail("invalid-email");
            if (result != "invalid-email")
            {
                problems.Add("MaskEmail: Invalid email format should return unchanged");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"MaskEmail: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate MaskPhoneNumber method
        try
        {
            var result = StringUtility.MaskPhoneNumber(null);
            if (result != string.Empty)
            {
                problems.Add("MaskPhoneNumber: Null input should return empty string");
            }

            result = StringUtility.MaskPhoneNumber("");
            if (result != string.Empty)
            {
                problems.Add("MaskPhoneNumber: Empty input should return empty string");
            }

            result = StringUtility.MaskPhoneNumber("123");
            if (result != "123")
            {
                problems.Add("MaskPhoneNumber: Input shorter than 4 chars should return unchanged");
            }

            result = StringUtility.MaskPhoneNumber("1234567890");
            if (result != "******7890")
            {
                problems.Add("MaskPhoneNumber: Should mask all but last 4 digits");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"MaskPhoneNumber: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate RemoveSpecialCharacters method
        try
        {
            var result = StringUtility.RemoveSpecialCharacters(null);
            if (result != string.Empty)
            {
                problems.Add("RemoveSpecialCharacters: Null input should return empty string");
            }

            result = StringUtility.RemoveSpecialCharacters("");
            if (result != string.Empty)
            {
                problems.Add("RemoveSpecialCharacters: Empty input should return empty string");
            }

            result = StringUtility.RemoveSpecialCharacters("hello world!");
            if (result != "hello world")
            {
                problems.Add("RemoveSpecialCharacters: Should remove punctuation but keep spaces");
            }

            result = StringUtility.RemoveSpecialCharacters("test@email.com");
            if (result != "testemailcom")
            {
                problems.Add("RemoveSpecialCharacters: Should remove all non-alphanumeric characters");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"RemoveSpecialCharacters: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        // Validate GenerateRandomString method
        try
        {
            var result = StringUtility.GenerateRandomString(10);
            if (result.Length != 10)
            {
                problems.Add("GenerateRandomString: Should generate string of correct length");
            }

            result = StringUtility.GenerateRandomString(0);
            if (result.Length != 0)
            {
                problems.Add("GenerateRandomString: Zero length should return empty string");
            }

            try
            {
                StringUtility.GenerateRandomString(-1);
                problems.Add("GenerateRandomString: Should throw for negative length");
            }
            catch (ArgumentOutOfRangeException)
            {
                // Expected behavior
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GenerateRandomString: Threw exception - {ex.GetType().Name}: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the StringUtility class is valid (has no validation problems).
    /// </summary>
    /// <param name="cultureInfo">The culture info to use for culture-sensitive operations. Defaults to InvariantCulture.</param>
    /// <returns>True if valid; false if any validation problems exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown if cultureInfo is null.</exception>
    public static bool IsValid(CultureInfo? cultureInfo = null) => Validate(cultureInfo).Count == 0;

    /// <summary>
    /// Ensures that the StringUtility class is valid, throwing an exception if not.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use for culture-sensitive operations. Defaults to InvariantCulture.</param>
    /// <exception cref="ArgumentNullException">Thrown if cultureInfo is null.</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing the list of problems.</exception>
    public static void EnsureValid(CultureInfo? cultureInfo = null)
    {
        var problems = Validate(cultureInfo);
        if (problems.Count == 0)
            return;

        throw new ArgumentException(
            $"StringUtility validation failed with {problems.Count} problem(s):{Environment.NewLine}" +
            string.Join($"{Environment.NewLine}", problems));
    }
}