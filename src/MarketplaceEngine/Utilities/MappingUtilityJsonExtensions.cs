#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using MarketplaceEngine.Domain.Models;
using MarketplaceEngine.DTOs;

namespace MarketplaceEngine.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for MappingUtility.
/// Enables easy serialization and deserialization of MappingUtility-related data.
/// </summary>
public static class MappingUtilityJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes a Listing domain model to a JSON string representation.
    /// </summary>
    /// <param name="listing">The Listing domain model to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the Listing.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listing"/> is null.</exception>
    public static string ToJson(this Listing listing, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(listing);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(listing, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a Listing domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized Listing instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static Listing? FromJsonToListing(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Listing>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a Listing domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized Listing instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToListing(string json, out Listing? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<Listing>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a User domain model to a JSON string representation.
    /// </summary>
    /// <param name="user">The User domain model to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the User.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is null.</exception>
    public static string ToJson(this User user, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(user);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(user, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a User domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized User instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static User? FromJsonToUser(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<User>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a User domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized User instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToUser(string json, out User? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<User>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a Message domain model to a JSON string representation.
    /// </summary>
    /// <param name="message">The Message domain model to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the Message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    public static string ToJson(this Message message, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(message);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(message, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a Message domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized Message instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static Message? FromJsonToMessage(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<Message>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a Message domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized Message instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToMessage(string json, out Message? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<Message>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a ModerationReport domain model to a JSON string representation.
    /// </summary>
    /// <param name="report">The ModerationReport domain model to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ModerationReport.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="report"/> is null.</exception>
    public static string ToJson(this ModerationReport report, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(report);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(report, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a ModerationReport domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized ModerationReport instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static ModerationReport? FromJsonToModerationReport(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ModerationReport>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ModerationReport domain model.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized ModerationReport instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToModerationReport(string json, out ModerationReport? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ModerationReport>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a ListingDto to a JSON string representation.
    /// </summary>
    /// <param name="dto">The ListingDto to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ListingDto.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    public static string ToJson(this ListingDto dto, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a ListingDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized ListingDto instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static ListingDto? FromJsonToListingDto(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ListingDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ListingDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized ListingDto instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToListingDto(string json, out ListingDto? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ListingDto>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a UserDto to a JSON string representation.
    /// </summary>
    /// <param name="dto">The UserDto to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the UserDto.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    public static string ToJson(this UserDto dto, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a UserDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized UserDto instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static UserDto? FromJsonToUserDto(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<UserDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a UserDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized UserDto instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToUserDto(string json, out UserDto? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<UserDto>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a MessageDto to a JSON string representation.
    /// </summary>
    /// <param name="dto">The MessageDto to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the MessageDto.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    public static string ToJson(this MessageDto dto, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a MessageDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized MessageDto instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static MessageDto? FromJsonToMessageDto(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<MessageDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a MessageDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized MessageDto instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToMessageDto(string json, out MessageDto? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<MessageDto>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Serializes a ModerationReportDto to a JSON string representation.
    /// </summary>
    /// <param name="dto">The ModerationReportDto to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ModerationReportDto.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dto"/> is null.</exception>
    public static string ToJson(this ModerationReportDto dto, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(dto, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a ModerationReportDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized ModerationReportDto instance, or null if JSON is empty or whitespace.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static ModerationReportDto? FromJsonToModerationReportDto(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ModerationReportDto>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ModerationReportDto.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized ModerationReportDto instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJsonToModerationReportDto(string json, out ModerationReportDto? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ModerationReportDto>(json, _jsonOptions);
            return value is not null;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}