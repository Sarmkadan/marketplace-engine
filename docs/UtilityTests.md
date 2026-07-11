# UtilityTests

Unit test class that validates the behavior of utility methods used throughout the marketplace engine. These tests ensure that email validation, price thresholds, input sanitization, text truncation, slug generation, email masking, and pagination helpers behave as expected under various inputs and edge cases.

## API

### `IsValidEmail_VariousInputs_ReturnsExpectedResult()`

Validates that the `IsValidEmail` method correctly identifies valid and invalid email formats across a range of inputs. The test covers standard email formats, edge cases like missing TLDs or special characters, and invalid patterns such as missing `@` or domain parts.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---

### `IsValidPrice_BelowMinimum_ReturnsFalse()`

Ensures that the price validation logic correctly rejects prices below the minimum allowed threshold. The test verifies behavior when the input price is zero, negative, or below the system-defined minimum.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---

### `SanitizeInput_WithNullControlCharacters_RemovesThem()`

Confirms that the `SanitizeInput` method removes or escapes null and control characters (e.g., `\0`, `\x01`–`\x1F`, `\x7F`) from user input. This prevents injection and ensures clean data processing.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---
### `Truncate_WhenTextExceedsMaxLength_TruncatesAndAppendsEllipsis()`

Tests that the `Truncate` method shortens long strings to a specified maximum length and appends an ellipsis (`…`) when truncation occurs. It verifies behavior with exact-length inputs, shorter inputs, and strings requiring truncation.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---
### `ToSlug_WithSpecialCharactersAndSpaces_ReturnsUrlFriendlySlug()`

Validates that the `ToSlug` method converts human-readable text (with spaces, punctuation, and special characters) into a URL-friendly slug. The test ensures proper handling of diacritics, case normalization, and removal of disallowed characters.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---
### `MaskEmail_WithTypicalEmail_MasksLocalPartAndPreservesDomain()`

Ensures that the `MaskEmail` method obfuscates the local part of an email address (e.g., `john.doe` → `j****e`) while preserving the domain (e.g., `@example.com`). It tests various email formats and edge cases like single-character local parts.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---
### `CalculateOffset_ForPage2WithSize10_Returns10()`

Confirms that the `CalculateOffset` method computes the correct zero-based offset for pagination. Given a page index and page size, it returns the starting index for the requested page. This test uses page 2 with a size of 10, expecting an offset of 10.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---
### `CalculateTotalPages_WithNonDivisibleTotal_CeilsUp()`

Validates that the `CalculateTotalPages` method rounds up the total number of pages when the total items are not evenly divisible by the page size. For example, 27 items with a page size of 10 should result in 3 total pages.

- **Parameters**: None
- **Return value**: `void`
- **Throws**: No exceptions expected under normal test conditions

---

## Usage
