using FreshMarket.Shared.Common;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FreshMarket.Application.Helpers;

public static class ExecutionHelper
{
    public static async Task<ServiceResult<T>> ExecuteAsync<T>(
        Func<Task<ServiceResult<T>>> action,
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
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Service Operation failed: {Operation} | Member: {Member} | File: {File} | Line: {Line} | Params: {@Params}",
                operation, memberName, Path.GetFileName(filePath), lineNumber, parameters);

            return ServiceResult<T>.Failure(
                ErrorCodes.System.UNEXPECTED,
                Messages.Get(MessageType.SystemProblem),
                HttpResponseStatus.InternalServerError);
        }
    }
}
