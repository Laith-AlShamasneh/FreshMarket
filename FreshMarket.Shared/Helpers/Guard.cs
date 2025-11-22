using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Minimal guard utility for argument validation in a beginner-friendly e-commerce project.
/// Each method throws a meaningful exception early and returns the validated value.
/// </summary>
public static class Guard
{
    /// <summary>Ensures a reference is not null.</summary>
    public static T AgainstNull<T>(
        T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : class
        => value ?? throw new ArgumentNullException(paramName, "Value cannot be null.");

    /// <summary>Ensures a value type is not its default value (e.g., 0, default struct).</summary>
    public static T AgainstDefault<T>(
        T value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : struct
        => EqualityComparer<T>.Default.Equals(value, default)
            ? throw new ArgumentException("Value cannot be the default value.", paramName)
            : value;

    /// <summary>Ensures a string is not null/empty/whitespace.</summary>
    public static string AgainstNullOrWhiteSpace(
        string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName)
            : value;

    /// <summary>Ensures an enumerable (collection, list, array) is not null or empty.</summary>
    public static IEnumerable<T> AgainstNullOrEmpty<T>(
        IEnumerable<T>? sequence,
        [CallerArgumentExpression(nameof(sequence))] string? paramName = null)
        => sequence is null || !sequence.Any()
            ? throw new ArgumentException("Collection cannot be null or empty.", paramName)
            : sequence;

    /// <summary>Ensures an int is greater than zero.</summary>
    public static int AgainstNegativeOrZero(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value <= 0
            ? throw new ArgumentException("Value must be greater than zero.", paramName)
            : value;

    /// <summary>Ensures a long is greater than zero.</summary>
    public static long AgainstNegativeOrZero(
        long value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value <= 0
            ? throw new ArgumentException("Value must be greater than zero.", paramName)
            : value;

    /// <summary>Ensures a decimal is greater than zero.</summary>
    public static decimal AgainstNegativeOrZero(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value <= 0m
            ? throw new ArgumentException("Value must be greater than zero.", paramName)
            : value;

    /// <summary>Ensures a Guid is not empty.</summary>
    public static Guid AgainstEmptyGuid(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value == Guid.Empty
            ? throw new ArgumentException("Guid cannot be empty.", paramName)
            : value;

    /// <summary>Ensures a value lies within the inclusive range [min, max].</summary>
    public static T AgainstOutOfRange<T>(
        T value,
        T min,
        T max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
        => value.CompareTo(min) < 0 || value.CompareTo(max) > 0
            ? throw new ArgumentOutOfRangeException(paramName, value,
                $"Value must be between {min} and {max} inclusive.")
            : value;

    /// <summary>Ensures an enum value is a defined member of its enum type.</summary>
    public static TEnum AgainstUndefinedEnum<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), value)
            ? value
            : throw new ArgumentOutOfRangeException(paramName, value,
                $"Value '{value}' is not a valid {typeof(TEnum).Name}.");

    /// <summary>Ensures a raw int maps to a defined enum member.</summary>
    public static int AgainstUndefinedEnumValue<TEnum>(
        int rawValue,
        [CallerArgumentExpression(nameof(rawValue))] string? paramName = null)
        where TEnum : struct, Enum
        => Enum.IsDefined(typeof(TEnum), rawValue)
            ? rawValue
            : throw new ArgumentOutOfRangeException(paramName, rawValue,
                $"Raw value '{rawValue}' is not defined for {typeof(TEnum).Name}.");

    /// <summary>Generic predicate-based guard. Throws if predicate returns true.</summary>
    public static T Against<T>(
        T value,
        Func<T, bool> predicate,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => predicate(value)
            ? throw new ArgumentException(message, paramName)
            : value;

    /// <summary>Requires a predicate be true; throws if false.</summary>
    public static T Require<T>(
        T value,
        Func<T, bool> condition,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => condition(value)
            ? value
            : throw new ArgumentException(message, paramName);
}
