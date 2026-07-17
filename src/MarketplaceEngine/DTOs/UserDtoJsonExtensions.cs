#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace MarketplaceEngine.DTOs;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="UserDto"/>.
/// </summary>
public static class UserDtoJsonExtensions
{
	private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
	};

	/// <summary>
	/// Serializes a <see cref="UserDto"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="UserDto"/> instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the <see cref="UserDto"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this UserDto value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_options) { WriteIndented = true }
			: _options;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="UserDto"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="UserDto"/> instance if deserialization succeeds; otherwise, null.</returns>
	/// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
	public static UserDto? FromJson(string json)
		=> string.IsNullOrWhiteSpace(json)
			? null
			: JsonSerializer.Deserialize<UserDto>(json, _options);

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="UserDto"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized <see cref="UserDto"/> instance if successful; otherwise, null.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string json, out UserDto? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = string.IsNullOrWhiteSpace(json)
			? null
			: JsonSerializer.Deserialize<UserDto>(json, _options);

		return value is not null;
	}
}