using System.ComponentModel;
using System.Reflection;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Minimal helper for working with enums in a beginner-friendly e-commerce project.
/// Provides safe parsing, listing, and display (via DescriptionAttribute) functionality.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Parses a string into enum T (case-insensitive). Throws if invalid.
    /// </summary>
    public static T Parse<T>(string value) where T : struct, Enum
        => TryParse(value, out T result)
            ? result
            : throw new ArgumentException($"Invalid {typeof(T).Name} value: '{value}'", nameof(value));

    /// <summary>
    /// Attempts to parse a string into enum T (case-insensitive).
    /// </summary>
    public static bool TryParse<T>(string? value, out T result) where T : struct, Enum
    {
        return Enum.TryParse(value?.Trim(), true, out result)
               && Enum.IsDefined(typeof(T), result);
    }

    /// <summary>
    /// Parses a string safely. Returns null instead of throwing if invalid.
    /// </summary>
    public static T? ParseOrNull<T>(string? value) where T : struct, Enum
        => TryParse(value, out T parsed) ? parsed : (T?)null;

    /// <summary>
    /// Returns all defined enum values.
    /// </summary>
    public static IReadOnlyList<T> GetValues<T>() where T : struct, Enum
        => [.. Enum.GetValues(typeof(T)).Cast<T>()];

    /// <summary>
    /// Returns (Value, Name) pairs for enum T. Useful for API dropdowns.
    /// </summary>
    public static IReadOnlyList<(T Value, string Name)> GetItems<T>() where T : struct, Enum
        => GetValues<T>().Select(v => (v, v.ToString())).ToList();

    /// <summary>
    /// Returns DescriptionAttribute text for an enum value if present; otherwise its name.
    /// </summary>
    public static string GetDescription<T>(T value) where T : struct, Enum
    {
        var name = value.ToString();
        var member = typeof(T).GetField(name);
        var desc = member?.GetCustomAttribute<DescriptionAttribute>();
        return desc?.Description ?? name;
    }

    /// <summary>
    /// Returns (Value, Description) pairs for enum T. Good for UI display.
    /// </summary>
    public static IReadOnlyList<(T Value, string Description)> GetDisplayItems<T>() where T : struct, Enum
        => GetValues<T>().Select(v => (v, GetDescription(v))).ToList();
}
