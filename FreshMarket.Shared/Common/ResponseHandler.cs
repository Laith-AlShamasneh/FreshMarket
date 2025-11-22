namespace FreshMarket.Shared.Common;

/// <summary>
/// Maps service results to API responses with localization.
/// </summary>
public class ResponseHandler(IUserContext userContext)
{
    private readonly IUserContext _user = userContext;

    public ApiResponse<T> Handle<T>(
        ServiceResult<T> result,
        MessageType successMessage = MessageType.RetrieveSuccessfully,
        MessageType failureMessage = MessageType.SystemProblem,
        HttpResponseStatus successStatus = HttpResponseStatus.OK,
        HttpResponseStatus failureStatus = HttpResponseStatus.BadRequest,
        string defaultErrorCode = ErrorCodes.System.UNEXPECTED)
    {
        var lang = _user.Lang;

        if (result.IsSuccess)
        {
            return ApiResponse<T>.Ok(
                result.Data!,
                successMessage,
                lang,
                successStatus);
        }

        // Map error message to an error code if you want smarter logic later
        var errorCode = defaultErrorCode;

        return ApiResponse<T>.Fail(
            errorCode,
            failureMessage,
            lang,
            failureStatus);
    }

    public async Task<ApiResponse<T>> HandleAsync<T>(
        Func<CancellationToken, Task<ServiceResult<T>>> serviceFunc,
        MessageType successMessage = MessageType.RetrieveSuccessfully,
        MessageType failureFallbackMessage = MessageType.SystemProblem,
        HttpResponseStatus successStatus = HttpResponseStatus.OK,
        HttpResponseStatus failureStatus = HttpResponseStatus.BadRequest,
        CancellationToken ct = default)
    {
        try
        {
            var serviceResult = await serviceFunc(ct);
            return Handle(serviceResult, successMessage, failureFallbackMessage, successStatus, failureStatus);
        }
        catch (OperationCanceledException)
        {
            return ApiResponse<T>.Fail(
                ErrorCodes.Common.UNKNOWN,
                MessageType.SystemProblem,
                _user.Lang,
                failureStatus);
        }
        catch (Exception)
        {
            return ApiResponse<T>.Fail(
                ErrorCodes.System.UNEXPECTED,
                failureFallbackMessage,
                _user.Lang,
                failureStatus);
        }
    }
}
