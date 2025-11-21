using System.ComponentModel;
using System.Reflection;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Provides safe and convenient utilities for working with enums: parsing, validation, enumeration, metadata extraction.
/// Designed for production scenarios (API dropdowns, input validation, display text mapping).
/// </summary>
public static class EnumHelper
{
    /// <summary>Parses the string to enum T (case-insensitive) or throws with a clear message.</summary>
    public static T Parse<T>(string value) where T : struct, Enum
        => TryParse(value, out T result)
            ? result
            : throw new ArgumentException($"Invalid {typeof(T).Name} value: '{value}'", nameof(value));

    /// <summary>Parses the string to enum T (case-insensitive); returns default(T) if invalid.</summary>
    public static T ParseOrDefault<T>(string value) where T : struct, Enum
        => TryParse(value, out T result) ? result : default;

    /// <summary>Parses the string to enum T (case-sensitive exact match) or throws if not found.</summary>
    public static T ParseExact<T>(string value) where T : struct, Enum
        => Enum.TryParse<T>(value, ignoreCase: false, out var parsed) && Enum.IsDefined(typeof(T), parsed)
            ? parsed
            : throw new ArgumentException($"Invalid exact {typeof(T).Name} value: '{value}'", nameof(value));

    /// <summary>Attempts to parse the string to enum T (case-insensitive).</summary>
    public static bool TryParse<T>(string value, out T result) where T : struct, Enum
    {
        result = default!;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var parsed)
               && Enum.IsDefined(typeof(T), parsed);
    }

    /// <summary>Attempts to parse an integer to enum T.</summary>
    public static bool TryParse<T>(int raw, out T result) where T : struct, Enum
    {
        result = default!;
        if (!Enum.IsDefined(typeof(T), raw))
            return false;

        result = (T)Enum.ToObject(typeof(T), raw);
        return true;
    }

    /// <summary>Returns true if the provided integer maps to a defined enum value.</summary>
    public static bool IsDefined<T>(int raw) where T : struct, Enum
        => Enum.IsDefined(typeof(T), raw);

    /// <summary>Returns true if the provided string maps to a defined enum name (case-insensitive).</summary>
    public static bool IsDefined<T>(string name) where T : struct, Enum
        => Enum.GetNames(typeof(T)).Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Throws if value is not a defined enum member.</summary>
    public static void RequireDefined<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(typeof(T), value))
            throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' is not a valid {typeof(T).Name}.");
    }

    /// <summary>Gets all enum values as a strongly typed list.</summary>
    public static IReadOnlyList<T> GetValues<T>() where T : struct, Enum
        => [.. Enum.GetValues(typeof(T)).Cast<T>()];

    /// <summary>Gets all enum names as a read-only list.</summary>
    public static IReadOnlyList<string> GetNames<T>() where T : struct, Enum
        => [.. Enum.GetNames(typeof(T))];

    /// <summary>Returns (Name, Value) pairs for enum T.</summary>
    public static IReadOnlyList<(string Name, T Value)> GetNameValuePairs<T>() where T : struct, Enum
        => GetValues<T>().Select(v => (v.ToString(), v)).ToList();

    /// <summary>Returns a dictionary mapping names to numeric values (int) of enum T.</summary>
    public static IReadOnlyDictionary<string, int> ToDictionary<T>() where T : struct, Enum
        => Enum.GetValues(typeof(T))
            .Cast<T>()
            .ToDictionary(v => v.ToString(), v => Convert.ToInt32(v));

    /// <summary>Gets DescriptionAttribute text if present; otherwise returns enum name.</summary>
    public static string GetDescription<T>(T value) where T : struct, Enum
    {
        var name = value.ToString();
        var field = typeof(T).GetField(name);
        if (field is null) return name;

        var attr = field.GetCustomAttribute<DescriptionAttribute>();
        return attr?.Description ?? name;
    }

    /// <summary>Attempts to get description for value; returns false if value not defined.</summary>
    public static bool TryGetDescription<T>(T value, out string description) where T : struct, Enum
    {
        description = string.Empty;
        if (!Enum.IsDefined(typeof(T), value))
            return false;

        description = GetDescription(value);
        return true;
    }

    /// <summary>Maps a list of integer raw values to defined enum values, skipping invalid ones.</summary>
    public static IReadOnlyList<T> FromValues<T>(IEnumerable<int> rawValues) where T : struct, Enum
        => rawValues
            .Where(IsDefined<T>)
            .Select(v => (T)Enum.ToObject(typeof(T), v))
            .ToList();

    /// <summary>Maps a list of names (case-insensitive) to defined enum values, skipping invalid ones.</summary>
    public static IReadOnlyList<T> FromNames<T>(IEnumerable<string> names) where T : struct, Enum
        => names
            .Where(n => TryParse<T>(n, out _))
            .Select(n => Parse<T>(n))
            .ToList();

    /// <summary>
    /// Converts a nullable enum to its underlying value or returns null if not set.
    /// Useful for serialization / API responses.
    /// </summary>
    public static int? ToNullableInt<T>(T? value) where T : struct, Enum
        => value.HasValue ? Convert.ToInt32(value.Value) : null;

    /// <summary>
    /// Safely converts an integer to a nullable enum (returns null if undefined).
    /// </summary>
    public static T? ToNullableEnum<T>(int? raw) where T : struct, Enum
        => raw.HasValue && IsDefined<T>(raw.Value)
            ? (T)Enum.ToObject(typeof(T), raw.Value)
            : null;
}
