# ModerationControllerValidation

A static validation utility that provides centralized, consistent validation logic for moderation-related controller inputs in the marketplace engine. It exposes paired methods—`Validate*`, `IsValid*`, and `EnsureValid*`—for different input categories such as pagination, report identifiers, action notes, rejection reasons, report reasons, and bulk moderation requests. The `Validate*` methods return a collection of error messages, `IsValid*` methods return a boolean indicating validity, and `EnsureValid*` methods throw an exception if validation fails.

## API

### General Validation

#### `Validate`
```csharp
public static IReadOnlyList<string> Validate(...)
```
Validates a complete moderation request input. Returns a read-only list of validation error messages. An empty list indicates the input is valid.

#### `IsValid`
```csharp
public static bool IsValid(...)
```
Returns `true` if the moderation request input passes all validation rules; otherwise, `false`.

#### `EnsureValid`
```csharp
public static void EnsureValid(...)
```
Throws an exception if the moderation request input is invalid. Does nothing if validation passes.

---

### Pagination Validation

#### `ValidatePagination`
```csharp
public static IReadOnlyList<string> ValidatePagination(...)
```
Validates pagination parameters. Returns a read-only list of error messages.

#### `IsValidPagination`
```csharp
public static bool IsValidPagination(...)
```
Returns `true` if the pagination parameters are valid; otherwise, `false`.

#### `EnsureValidPagination`
```csharp
public static void EnsureValidPagination(...)
```
Throws an exception if pagination parameters are invalid.

---

### Report ID Validation

#### `ValidateReportId`
```csharp
public static IReadOnlyList<string> ValidateReportId(...)
```
Validates a report identifier. Returns a read-only list of error messages.

#### `IsValidReportId`
```csharp
public static bool IsValidReportId(...)
```
Returns `true` if the report ID is valid; otherwise, `false`.

#### `EnsureValidReportId`
```csharp
public static void EnsureValidReportId(...)
```
Throws an exception if the report ID is invalid.

---

### Action Notes Validation

#### `ValidateActionNotes`
```csharp
public static IReadOnlyList<string> ValidateActionNotes(...)
```
Validates moderation action notes. Returns a read-only list of error messages.

#### `IsValidActionNotes`
```csharp
public static bool IsValidActionNotes(...)
```
Returns `true` if the action notes are valid; otherwise, `false`.

#### `EnsureValidActionNotes`
```csharp
public static void EnsureValidActionNotes(...)
```
Throws an exception if the action notes are invalid.

---

### Rejection Reason Validation

#### `ValidateRejectionReason`
```csharp
public static IReadOnlyList<string> ValidateRejectionReason(...)
```
Validates a rejection reason. Returns a read-only list of error messages.

#### `IsValidRejectionReason`
```csharp
public static bool IsValidRejectionReason(...)
```
Returns `true` if the rejection reason is valid; otherwise, `false`.

#### `EnsureValidRejectionReason`
```csharp
public static void EnsureValidRejectionReason(...)
```
Throws an exception if the rejection reason is invalid.

---

### Report Reason Validation

#### `ValidateReportReason`
```csharp
public static IReadOnlyList<string> ValidateReportReason(...)
```
Validates a report reason. Returns a read-only list of error messages.

#### `IsValidReportReason`
```csharp
public static bool IsValidReportReason(...)
```
Returns `true` if the report reason is valid; otherwise, `false`.

#### `EnsureValidReportReason`
```csharp
public static void EnsureValidReportReason(...)
```
Throws an exception if the report reason is invalid.

---

### Bulk Moderation Validation

#### `ValidateBulkModeration`
```csharp
public static IReadOnlyList<string> ValidateBulkModeration(...)
```
Validates a bulk moderation request. Returns a read-only list of error messages.

#### `IsValidBulkModeration`
```csharp
public static bool IsValidBulkModeration(...)
```
Returns `true` if the bulk moderation request is valid; otherwise, `false`.

---

## Usage

### Example 1: Using Validate and EnsureValid for a moderation action

```csharp
var errors = ModerationControllerValidation.ValidateActionNotes(actionNotes);

if (errors.Any())
{
    foreach (var error in errors)
    {
        _logger.LogWarning("Validation error: {Error}", error);
    }
    return BadRequest(errors);
}

ModerationControllerValidation.EnsureValidActionNotes(actionNotes);
_service.ApplyModerationAction(reportId, actionNotes);
```

### Example 2: Guard clause with IsValid pattern

```csharp
if (!ModerationControllerValidation.IsValidReportId(reportId))
{
    throw new ArgumentException("Invalid report identifier.", nameof(reportId));
}

if (!ModerationControllerValidation.IsValidBulkModeration(bulkRequest))
{
    var details = ModerationControllerValidation.ValidateBulkModeration(bulkRequest);
    throw new ValidationException(string.Join("; ", details));
}

await _moderationService.ProcessBulkAsync(bulkRequest);
```

---

## Notes

- All methods are static and stateless, making them safe to call concurrently from multiple threads without external synchronization.
- The `Validate*` methods never return `null`; they return an empty read-only list when no errors are found.
- The `EnsureValid*` methods throw on the first invalid condition encountered; the exact exception type and message depend on the internal implementation and the specific validation failure.
- Input parameters are not documented here explicitly, but each method group corresponds to a distinct input type (e.g., a report ID, a pagination object, a bulk moderation request). Passing `null` to any method will likely result in a validation error or an exception, depending on the implementation.
- The `Validate*` methods are designed for scenarios where callers need to collect all errors at once (e.g., returning multiple validation messages to a client), while `IsValid*` provides a quick boolean check, and `EnsureValid*` is suited for guard clauses that should fail fast.
