using System.Text.Json.Serialization;

namespace FreshMarket.Shared.Helpers;

///// <summary>
///// Represents the outcome of an operation — either success or failure with an error message.  
///// Enables functional-style error handling without exceptions. Supports pattern matching and chaining.
///// </summary>
//public record Result(bool IsSuccess, string? Error = null)
//{
//    public static Result Success() => new(true);
//    public static Result Failure(string error) => new(false, error);

//    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
//        => IsSuccess ? onSuccess() : onFailure(Error!);
//}

//public record Result<T>(bool IsSuccess, T? Value = default, string? Error = null)
//{
//    public static Result<T> Success(T value) => new(true, value);
//    public static Result<T> Failure(string error) => new(false, default, error);

//    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
//        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);

//    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
//        => IsSuccess ? binder(Value!) : Result<TNew>.Failure(Error!);
//}


public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyList<ErrorDetail> Errors { get; }

    [JsonIgnore] public Exception? Exception { get; }

    private Result(
        bool isSuccess,
        string? errorCode,
        string? errorMessage,
        IReadOnlyList<ErrorDetail>? errors,
        Exception? exception)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Errors = errors ?? [];
        Exception = exception;
    }

    // Factory: Success
    public static Result Success() => new(true, null, null, null, null);

    // Factory: Failure (single)
    public static Result Failure(string errorCode, string errorMessage)
        => new(false, errorCode, errorMessage, new[] { new ErrorDetail(errorCode, errorMessage) }, null);

    // Factory: Failure (multiple)
    public static Result Failure(IEnumerable<ErrorDetail> errors)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Failure("EMPTY_ERROR", "Unknown failure")
            : new Result(false, list[0].Code, list[0].Message, list, null);
    }

    // Factory: Failure (exception)
    public static Result Failure(Exception ex, string? overrideMessage = null)
        => new(false, ex.GetType().Name, overrideMessage ?? ex.Message, new[] { new ErrorDetail(ex.GetType().Name, overrideMessage ?? ex.Message) }, ex);

    // Combine multiple results
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        return failures.Count == 0
            ? Success()
            : Failure(failures.SelectMany(f => f.Errors));
    }

    // Convert to generic Result<T> with explicit value
    public Result<T> ToResult<T>(T value) => IsSuccess
        ? Result<T>.Success(value)
        : Result<T>.Failure(Errors);

    public override string ToString()
        => IsSuccess ? "Success" : $"Failure: {ErrorCode} - {ErrorMessage}";
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type T.
/// </summary>
/// <typeparam name="T">The value type returned on success.</typeparam>
public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public IReadOnlyList<ErrorDetail> Errors { get; }
    [JsonIgnore] public Exception? Exception { get; }

    private Result(
        bool isSuccess,
        T? value,
        string? errorCode,
        string? errorMessage,
        IReadOnlyList<ErrorDetail>? errors,
        Exception? exception)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Errors = errors ?? Array.Empty<ErrorDetail>();
        Exception = exception;
    }

    // Factory: Success
    public static Result<T> Success(T value)
        => new(true, value, null, null, null, null);

    // Factory: Failure (single)
    public static Result<T> Failure(string errorCode, string errorMessage)
        => new(false, default, errorCode, errorMessage, new[] { new ErrorDetail(errorCode, errorMessage) }, null);

    // Factory: Failure (multiple)
    public static Result<T> Failure(IEnumerable<ErrorDetail> errors)
    {
        var list = errors?.ToList() ?? [];
        return list.Count == 0
            ? Failure("EMPTY_ERROR", "Unknown failure")
            : new Result<T>(false, default, list[0].Code, list[0].Message, list, null);
    }

    // Factory: Failure (exception)
    public static Result<T> Failure(Exception ex, string? overrideMessage = null)
        => new(false, default, ex.GetType().Name, overrideMessage ?? ex.Message, new[] { new ErrorDetail(ex.GetType().Name, overrideMessage ?? ex.Message) }, ex);

    // Combine many Result<T> -> Result<IReadOnlyList<T>>
    public static Result<IReadOnlyList<T>> Combine(params Result<T>[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();
        if (failures.Count > 0)
            return Result<IReadOnlyList<T>>.Failure(failures.SelectMany(f => f.Errors));

        return Result<IReadOnlyList<T>>.Success(results.Select(r => r.Value!).ToList());
    }

    // Map on success
    public Result<TResult> Map<TResult>(Func<T, TResult> selector)
    {
        if (IsFailure) return Result<TResult>.Failure(Errors);
        try
        {
            return Result<TResult>.Success(selector(Value!));
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(ex);
        }
    }

    // Bind (flatMap) for chaining
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> binder)
    {
        if (IsFailure) return Result<TResult>.Failure(Errors);
        try
        {
            return binder(Value!);
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure(ex);
        }
    }

    // Tap: perform side-effect if success
    public Result<T> Tap(Action<T> action)
    {
        if (IsSuccess)
        {
            try { action(Value!); }
            catch (Exception ex) { return Failure(ex); }
        }
        return this;
    }

    // Ensure: apply predicate on success; if predicate fails return failure
    public Result<T> Ensure(Func<T, bool> predicate, string errorCode, string errorMessage)
        => IsFailure
            ? this
            : predicate(Value!)
                ? this
                : Failure(errorCode, errorMessage);

    // Convert to non-generic result (dropping Value)
    public Result ToResult()
        => IsSuccess ? Result.Success() : Result.Failure(Errors);

    // Deconstruct support (useful in patterns)
    public void Deconstruct(out bool isSuccess, out T? value)
    {
        isSuccess = IsSuccess;
        value = Value;
    }

    public override string ToString()
        => IsSuccess ? $"Success<{typeof(T).Name}>({Value})" : $"Failure<{typeof(T).Name}>: {ErrorCode} - {ErrorMessage}";

    // Implicit conversion from T (allow: Result<string> r = "abc";)
    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Represents a single error item used inside a Result.
/// </summary>
public sealed record ErrorDetail(string Code, string Message)
{
    public override string ToString() => $"{Code}: {Message}";
}

/// <summary>
/// Common shortcut extensions for working with Result/Result&lt;T&gt;.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts an exception into a failure Result (non-generic).
    /// </summary>
    public static Result ToFailure(this Exception ex, string? overrideMessage = null)
        => Result.Failure(ex, overrideMessage);

    /// <summary>
    /// Converts an exception into a failure Result&lt;T&gt;.
    /// </summary>
    public static Result<T> ToFailure<T>(this Exception ex, string? overrideMessage = null)
        => Result<T>.Failure(ex, overrideMessage);

    /// <summary>
    /// Converts a sequence of ErrorDetail to a non-generic Result failure.
    /// </summary>
    public static Result ToFailure(this IEnumerable<ErrorDetail> errors)
        => Result.Failure(errors);

    /// <summary>
    /// Converts a sequence of ErrorDetail to a generic Result&lt;T&gt; failure.
    /// </summary>
    public static Result<T> ToFailure<T>(this IEnumerable<ErrorDetail> errors)
        => Result<T>.Failure(errors);

    /// <summary>
    /// Unwraps success value or throws ArgumentException with combined error info.
    /// </summary>
    public static T Unwrap<T>(this Result<T> result)
        => result.IsSuccess
            ? result.Value!
            : throw new ArgumentException(
                $"Result was failure. Errors: {string.Join(", ", result.Errors.Select(e => e.ToString()))}");

    /// <summary>
    /// Gets success value or provided fallback if failure.
    /// </summary>
    public static T GetOr<T>(this Result<T> result, T fallback)
        => result.IsSuccess ? result.Value! : fallback;

    /// <summary>
    /// Gets success value or invokes factory for fallback lazily.
    /// </summary>
    public static T GetOrEval<T>(this Result<T> result, Func<T> fallbackFactory)
        => result.IsSuccess ? result.Value! : fallbackFactory();
}