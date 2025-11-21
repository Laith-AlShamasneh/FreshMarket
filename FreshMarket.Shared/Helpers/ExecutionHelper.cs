using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Centralized execution helpers for synchronous and asynchronous operations:
/// - Standardized structured logging
/// - Optional timing, retry, cancellation
/// - Optional exception classification
/// Keeps original simple Execute methods while offering advanced overloads.
/// </summary>
public static class ExecutionHelper
{
    #region Basic (Existing)

    public static void Execute(
        Action action,
        ILogger logger,
        string operation,
        object? parameters = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        try
        {
            action();
        }
        catch (Exception ex)
        {
            LogError(logger, ex, operation, memberName, filePath, lineNumber, parameters, isAsync: false);
            throw;
        }
    }

    public static async Task ExecuteAsync(
        Func<Task> action,
        ILogger logger,
        string operation,
        object? parameters = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            LogError(logger, ex, operation, memberName, filePath, lineNumber, parameters, isAsync: true);
            throw;
        }
    }

    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        ILogger logger,
        string operation,
        object? parameters = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            LogError(logger, ex, operation, memberName, filePath, lineNumber, parameters, isAsync: true);
            throw;
        }
    }

    #endregion

    #region Enhanced Variants

    /// <summary>
    /// Executes an async action with timing and optional classification of exceptions.
    /// </summary>
    public static async Task<T> ExecuteTimedAsync<T>(
        Func<Task<T>> action,
        ILogger logger,
        string operation,
        object? parameters = null,
        bool logSuccess = true,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        var sw = Stopwatch.StartNew();
        try
        {
            var result = await action();
            sw.Stop();
            if (logSuccess)
            {
                logger.LogInformation(
                    "Operation succeeded: {Operation} ({Elapsed} ms) | Member: {Member} | File: {File} | Line: {Line} | Params: {@Params}",
                    operation, sw.ElapsedMilliseconds, memberName, Path.GetFileName(filePath), lineNumber, parameters);
            }
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            LogError(logger, ex, operation, memberName, filePath, lineNumber, parameters, isAsync: true, sw.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Executes an async action with retry for transient failures.
    /// Basic exponential backoff (2^attempt * baseDelay).
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> action,
        ILogger logger,
        string operation,
        int maxAttempts = 3,
        int baseDelayMs = 200,
        Func<Exception, bool>? isTransient = null,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);

        isTransient ??= ex => ex is TimeoutException || ex.Message.Contains("transient", StringComparison.OrdinalIgnoreCase);

        int attempt = 0;
        Exception? lastException = null;

        while (attempt < maxAttempts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (!isTransient(ex) || attempt == maxAttempts)
                {
                    LogError(logger, ex, $"{operation} (attempt {attempt}/{maxAttempts})",
                        memberName, filePath, lineNumber, parameters, isAsync: true);
                    throw;
                }

                var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt - 1));
                logger.LogWarning(
                    ex,
                    "Transient failure on {Operation} attempt {Attempt}/{Max}. Retrying in {Delay} ms...",
                    operation, attempt, maxAttempts, delay.TotalMilliseconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        // should never reach here as final attempt either succeeds or throws
        throw lastException ?? new InvalidOperationException("Unknown failure in retry block.");
    }

    /// <summary>
    /// Executes an async action safely returning a success flag instead of throwing.
    /// Useful for non-critical logging / secondary operations.
    /// </summary>
    public static async Task<(bool Success, Exception? Error)> TryExecuteAsync(
        Func<Task> action,
        ILogger logger,
        string operation,
        object? parameters = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        try
        {
            await action();
            return (true, null);
        }
        catch (Exception ex)
        {
            LogError(logger, ex, operation, memberName, filePath, lineNumber, parameters, isAsync: true);
            return (false, ex);
        }
    }

    #endregion

    #region Internal Logging Helper

    private static void LogError(
        ILogger logger,
        Exception ex,
        string operation,
        string memberName,
        string filePath,
        int lineNumber,
        object? parameters,
        bool isAsync,
        long? elapsedMs = null)
    {
        var template = isAsync
            ? "Async operation failed: {Operation} | Member: {Member} | File: {File} | Line: {Line} | Elapsed: {Elapsed} | Params: {@Params}"
            : "Sync operation failed: {Operation} | Member: {Member} | File: {File} | Line: {Line} | Elapsed: {Elapsed} | Params: {@Params}";

        logger.LogError(
            ex,
            template,
            operation,
            memberName,
            Path.GetFileName(filePath),
            lineNumber,
            elapsedMs.HasValue ? $"{elapsedMs.Value} ms" : "n/a",
            parameters);
    }

    #endregion
}
