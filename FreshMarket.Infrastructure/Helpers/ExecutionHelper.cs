using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FreshMarket.Infrastructure.Helpers;

/// <summary>
/// Very simple centralized execution helper:
/// - Wraps operations in try/catch
/// - Logs failures with caller context
/// - Re-throws to preserve stack
/// Keep it minimal for readability.
/// </summary>
public static class ExecutionHelper
{
    /// <summary>
    /// Executes an asynchronous function that returns a result with standardized error logging.
    /// </summary>
    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        ILogger logger,
        string operation,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await action();
        }
        catch (Exception ex) when (LogAndRethrow(ex, logger, operation, memberName, filePath, lineNumber, parameters))
        {
            throw;
        }
    }

    /// <summary>
    /// Executes an asynchronous action with standardized error logging.
    /// </summary>
    public static async Task ExecuteAsync(
        Func<Task> action,
        ILogger logger,
        string operation,
        object? parameters = null,
        CancellationToken cancellationToken = default,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await action();
        }
        catch (Exception ex) when (LogAndRethrow(ex, logger, operation, memberName, filePath, lineNumber, parameters))
        {
            throw;
        }
    }

    /// <summary>
    /// Executes a synchronous action with standardized error logging.
    /// </summary>
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
        ArgumentNullException.ThrowIfNull(action);

        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Operation failed: {Operation} | Member: {Member} | File: {File} | Line: {Line} | Params: {@Params}",
                operation, memberName, Path.GetFileName(filePath), lineNumber, parameters);
            throw;
        }
    }

    /// <summary>
    /// Logging helper used in exception filters to avoid duplicate try/catch blocks.
    /// </summary>
    private static bool LogAndRethrow(
        Exception ex,
        ILogger logger,
        string operation,
        string memberName,
        string filePath,
        int lineNumber,
        object? parameters)
    {
        logger.LogError(
            ex,
            "Operation failed: {Operation} | Member: {Member} | File: {File} | Line: {Line} | Params: {@Params}",
            operation, memberName, Path.GetFileName(filePath), lineNumber, parameters);
        return false;
    }
}
