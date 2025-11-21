namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Represents the outcome of an operation — either success or failure with an error message.  
/// Enables functional-style error handling without exceptions. Supports pattern matching and chaining.
/// </summary>
public record Result(bool IsSuccess, string? Error = null)
{
    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);

    public TResult Match<TResult>(Func<TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Error!);
}

public record Result<T>(bool IsSuccess, T? Value = default, string? Error = null)
{
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(string error) => new(false, default, error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);

    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
        => IsSuccess ? binder(Value!) : Result<TNew>.Failure(Error!);
}
