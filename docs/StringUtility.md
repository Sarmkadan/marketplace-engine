# StringUtility

A static utility class providing common string manipulation operations for formatting, sanitization, masking, and generation. Designed to centralize reusable string transformations within the marketplace-engine application.

## API

### Truncate

**Purpose**: Shortens a string to a specified maximum length, appending an ellipsis if truncated.

**Parameters**:
- `value` (string): The input string to truncate.
- `maxLength` (int): The maximum allowed length of the output string.

**Return Value**: The truncated string with "..." appended if the input exceeds `maxLength`. Returns the original string if within limits.

**Exceptions**:
- `ArgumentNullException`: Thrown when `value` is null.
- `ArgumentOutOfRangeException`: Thrown when `maxLength` is less than 3.

---

### ToTitleCase

**Purpose**: Converts the first character of each word in a string to uppercase, with remaining characters lowercase.

**Parameters**:
- `value` (string): The input string to convert.

**Return Value**: The title-cased string. Words are delimited by whitespace.

**Exceptions**:
- `ArgumentNullException`: Thrown when `value` is null.

---

### ToSlug

**Purpose**: Transforms a string into a URL-friendly slug format (lowercase, hyphen-separated, special characters removed).

**Parameters**:
- `value` (string): The input string to convert.

**Return Value**: A slug-formatted string suitable for URLs or identifiers.

**Exceptions**:
- `ArgumentNullException`: Thrown when `value` is null.

---

### Repeat

**Purpose**: Repeats a string a specified number of times, concatenating results.

**Parameters**:
- `value` (string): The string to repeat.
- `count` (int): The number of repetitions.

**Return Value**: The repeated string. Returns empty string if `count` is 0.

**Exceptions**:
- `ArgumentNullException`: Thrown when `value` is null.
- `ArgumentOutOfRangeException`: Thrown when `count` is negative.

---

### ContainsAny

**Purpose**: Determines whether a string contains any of the specified substrings.

**Parameters**:
- `value` (string): The source string to search.
- `values` (string[]): Array of substrings to check for.

**Return Value**: `true` if any substring in `values` is found in `value`; otherwise `false`.

**Exceptions**:
- `ArgumentNullException`: Thrown when `value` or `values` is null.

---

### MaskEmail

**Purpose**: Obfuscates an email address by replacing characters with asterisks, preserving domain structure.

**Parameters**:
- `email` (string): The email address to mask.

**Return Value**: The masked email (e.g., "j***@example.com").

**Exceptions**:
- `ArgumentNullException`: Thrown when `email` is null.
- `FormatException`: Thrown when `email` is not a valid email format.

---

### MaskPhoneNumber

**Purpose**: Masks a phone number, retaining only the last four digits.

**Parameters**:
- `phoneNumber` (string): The phone number to mask.

**Return Value**: The masked phone number (e.g., "***-***-1234").

**Exceptions**:
- `ArgumentNullException`: Thrown when `phoneNumber` is null.
- `FormatException`: Thrown when `phoneNumber` is not a valid phone number format.

---

### RemoveSpecialCharacters

**Purpose**: Strips non-alphanumeric characters from a string, preserving spaces.

**Parameters**:
- `value` (string): The input string to clean.

**Return Value**: The sanitized string containing only letters, numbers, and spaces.

**Exceptions**:
- `ArgumentNullException`: Thrown when `value` is null.

---

### GenerateRandomString

**Purpose**: Produces a cryptographically random string of specified length using alphanumeric characters.

**Parameters**:
- `length` (int): The desired length of the output string.

**Return Value**: A random string of the specified length.

**Exceptions**:
- `ArgumentOutOfRangeException`: Thrown when `length` is negative.

---

## Usage

```csharp
// Truncate product descriptions for display
string description = "This is a very long product description that needs to be shortened.";
string truncated = StringUtility.Truncate(description, 20);
// Result: "This is a very long..."

// Generate SEO-friendly slugs from category names
string category = "Electronics & Gadgets!";
string slug = StringUtility.ToSlug(category);
// Result: "electronics-gadgets"
```

```csharp
// Mask sensitive user contact information
string email = "john.doe@example.com";
string maskedEmail = StringUtility.MaskEmail(email);
// Result: "j***@example.com"

string phone = "+1 (555) 123-4567";
string maskedPhone = StringUtility.MaskPhoneNumber(phone);
// Result: "***-***-4567"
```

---

## Notes

- All methods throw `ArgumentNullException` when required string parameters are null, except `ContainsAny`, which also checks the `values` array.
- `Truncate` requires `maxLength` to be at least 3 to accommodate the ellipsis suffix.
- `ToSlug` removes diacritics and converts to lowercase, which may alter the semantic meaning of certain Unicode strings.
- `GenerateRandomString` uses `RNGCryptoServiceProvider` internally and is suitable for security-sensitive contexts.
- All methods are thread-safe as they do not maintain state or rely on mutable shared resources.
