namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Provides safe and convenient utilities for working with enums: parsing, validation, and enumeration.
/// </summary>
public static class EnumHelper
{
    public static T Parse<T>(string value) where T : struct, Enum
        => TryParse<T>(value, out var result)
            ? result
            : throw new ArgumentException($"Invalid {typeof(T).Name}: {value}");

    public static bool TryParse<T>(string value, out T result) where T : struct, Enum
    {
        result = default!;
        return Enum.TryParse<T>(value, ignoreCase: true, out var parsed)
               && Enum.IsDefined(typeof(T), parsed);
    }

    public static IReadOnlyList<T> GetValues<T>() where T : struct, Enum
        => [.. Enum.GetValues(typeof(T)).Cast<T>()];
}
