using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Provides fluent guard clauses to validate method arguments and throw meaningful exceptions early.  
/// Improves code readability and prevents null/reference bugs at runtime.
/// </summary>
public static class Guard
{
    public static T AgainstNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value ?? throw new ArgumentNullException(paramName, "Value cannot be null.");

    public static T AgainstNullOrEmpty<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IEnumerable<object>
        => value is null || !value.Any()
            ? throw new ArgumentException("Collection cannot be null or empty.", paramName)
            : value;

    public static string AgainstNullOrWhiteSpace(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName)
            : value;

    public static T AgainstDefault<T>(
        T value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : struct
        => EqualityComparer<T>.Default.Equals(value, default)
            ? throw new ArgumentException("Value cannot be the default value.", paramName)
            : value;

    public static long AgainstZero(
        long value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value == 0
            ? throw new ArgumentException("Value cannot be zero.", paramName)
            : value;

    public static long AgainstNegative(
        long value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value < 0
            ? throw new ArgumentException("Value cannot be negative.", paramName)
            : value;

    public static long AgainstNegativeOrZero(
        long value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value <= 0
            ? throw new ArgumentException("Value cannot be negative or zero.", paramName)
            : value;

    public static decimal AgainstNegativeOrZero(
    decimal value,
    [CallerArgumentExpression(nameof(value))] string? paramName = null)
    => value <= 0m
        ? throw new ArgumentException("Value cannot be negative or zero.", paramName)
        : value;

    public static int AgainstOutOfRange(
        int value,
        int min,
        int max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value < min || value > max
            ? throw new ArgumentOutOfRangeException(
                paramName,
                value,
                $"Value must be between {min} and {max}, inclusive.")
            : value;

    public static Guid AgainstEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value == Guid.Empty
            ? throw new ArgumentException("Guid cannot be empty.", paramName)
            : value;

    public static Guid AgainstEmpty(
        [NotNull] Guid? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value is null || value == Guid.Empty
            ? throw new ArgumentException("Guid cannot be null or empty.", paramName)
            : value.Value;
}
