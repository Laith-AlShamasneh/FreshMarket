using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Provides fluent guard clauses to validate arguments early, producing meaningful exceptions.
/// Supports primitives, collections, enums, Date/Time semantics, and custom predicates.
/// </summary>
public static class Guard
{
    #region Existing Methods (Unchanged)

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

    #endregion

    #region Added Numeric Variants

    public static int AgainstNegativeOrZero(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value <= 0
            ? throw new ArgumentException("Value cannot be negative or zero.", paramName)
            : value;

    public static double AgainstNegativeOrZero(
        double value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value <= 0d
            ? throw new ArgumentException("Value cannot be negative or zero.", paramName)
            : value;

    public static decimal AgainstNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value < 0m
            ? throw new ArgumentException("Value cannot be negative.", paramName)
            : value;

    public static int AgainstNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value < 0
            ? throw new ArgumentException("Value cannot be negative.", paramName)
            : value;

    public static double AgainstNegative(
        double value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value < 0d
            ? throw new ArgumentException("Value cannot be negative.", paramName)
            : value;

    #endregion

    #region Collections / Sequences

    public static IReadOnlyCollection<T> AgainstNullOrEmpty<T>(
        [NotNull] IReadOnlyCollection<T>? collection,
        [CallerArgumentExpression(nameof(collection))] string? paramName = null)
        => collection is null || collection.Count == 0
            ? throw new ArgumentException("Collection cannot be null or empty.", paramName)
            : collection;

    public static IEnumerable<T> AgainstEmpty<T>(
        IEnumerable<T> sequence,
        [CallerArgumentExpression(nameof(sequence))] string? paramName = null)
        => sequence is null || !sequence.Any()
            ? throw new ArgumentException("Sequence cannot be null or empty.", paramName)
            : sequence;

    #endregion

    #region Enum Validations

    public static TEnum AgainstUndefinedEnum<TEnum>(
        TEnum value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where TEnum : struct, Enum
        => !Enum.IsDefined(typeof(TEnum), value)
            ? throw new ArgumentOutOfRangeException(paramName, value, $"Value '{value}' is not a valid {typeof(TEnum).Name}.")
            : value;

    public static int AgainstUndefinedEnumValue<TEnum>(
        int rawValue,
        [CallerArgumentExpression(nameof(rawValue))] string? paramName = null)
        where TEnum : struct, Enum
        => !Enum.IsDefined(typeof(TEnum), rawValue)
            ? throw new ArgumentOutOfRangeException(paramName, rawValue, $"Raw value '{rawValue}' is not defined for {typeof(TEnum).Name}.")
            : rawValue;

    #endregion

    #region Date / Time Semantics

    public static DateTime AgainstDefault(
        DateTime value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value == default
            ? throw new ArgumentException("DateTime cannot be default.", paramName)
            : value;

    public static DateTime AgainstFuture(
        DateTime value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value > DateTime.UtcNow
            ? throw new ArgumentException("DateTime cannot be in the future.", paramName)
            : value;

    public static DateTime AgainstPast(
        DateTime value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value < DateTime.UtcNow
            ? throw new ArgumentException("DateTime cannot be in the past.", paramName)
            : value;

    public static DateOnly AgainstDefault(
        DateOnly value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value == default
            ? throw new ArgumentException("DateOnly cannot be default.", paramName)
            : value;

    public static TimeSpan AgainstNegativeOrZero(
        TimeSpan value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value <= TimeSpan.Zero
            ? throw new ArgumentException("TimeSpan must be positive.", paramName)
            : value;

    #endregion

    #region Range / Between

    public static T AgainstNotInRange<T>(
        T value,
        T min,
        T max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        where T : IComparable<T>
        => value.CompareTo(min) < 0 || value.CompareTo(max) > 0
            ? throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max} inclusive.")
            : value;

    public static decimal AgainstNotInRange(
        decimal value,
        decimal min,
        decimal max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value < min || value > max
            ? throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max} inclusive.")
            : value;

    #endregion

    #region Custom Predicate

    public static T Against<T>(
        T value,
        Func<T, bool> predicate,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => predicate(value)
            ? throw new ArgumentException(message, paramName)
            : value;

    public static T Require<T>(
        T value,
        Func<T, bool> condition,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => !condition(value)
            ? throw new ArgumentException(message, paramName)
            : value;

    #endregion

    #region Optional Custom Message Versions

    public static string AgainstNullOrWhiteSpace(
        [NotNull] string? value,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(message, paramName)
            : value;

    public static T AgainstNull<T>(
        [NotNull] T? value,
        string message,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
        => value ?? throw new ArgumentNullException(paramName, message);

    #endregion
}
