using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FreshMarket.Shared.Helpers;

/// <summary>
/// Provides centralized async execution with automatic structured logging and exception re-throwing.  
/// Eliminates repetitive try/catch blocks while preserving stack traces and enriching logs with caller context.
/// </summary>
public static class ExecutionHelper
{
    public static void Execute(
    Action action,
    ILogger logger,
    string operation,
    object? parameters = null,
    [CallerMemberName] string memberName = "",
    [CallerFilePath] string filePath = "",
    [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Sync operation failed: {Operation} | Member: {Member} | File: {File} | Line: {Line} | Params: {@Params}",
                operation, memberName, Path.GetFileName(filePath), lineNumber, parameters);
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
        try
        {
            await action();
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

    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        ILogger logger,
        string operation,
        object? parameters = null,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            return await action();
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
}
