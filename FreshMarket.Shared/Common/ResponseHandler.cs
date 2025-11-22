namespace FreshMarket.Shared.Common;

public class ResponseHandler(IUserContext userContext)
{
    private readonly IUserContext _userContext = userContext;

    public ApiResponse<T> Handle<T>(
        ServiceResult<T> result,
        MessageType successMessage = MessageType.RetrieveSuccessfully,
        MessageType failureFallbackMessage = MessageType.SystemProblem,
        HttpResponseStatus successStatus = HttpResponseStatus.OK,
        HttpResponseStatus failureStatus = HttpResponseStatus.BadRequest)
    {
        var lang = _userContext.Lang;

        if (result.IsSuccess)
        {
            return ApiResponse<T>.Ok(
                result.Data!,
                successMessage,
                lang,
                successStatus);
        }

        var message = result.ErrorMessage ?? Messages.Get(failureFallbackMessage, lang);

        return ApiResponse<T>.Fail(
            result.ErrorMessage ?? "ERROR",
            message,
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
            var result = await serviceFunc(ct);
            return Handle(result, successMessage, failureFallbackMessage, successStatus, failureStatus);
        }
        catch (Exception)
        {
            var lang = _userContext.Lang;
            return ApiResponse<T>.Fail(
                "SYSTEM_UNEXPECTED",
                Messages.Get(failureFallbackMessage, lang),
                failureStatus);
        }
    }

    public Task<ApiResponse<T>> HandleAsync<T>(
        Func<Task<ServiceResult<T>>> serviceFunc,
        MessageType successMessage = MessageType.RetrieveSuccessfully,
        MessageType failureFallbackMessage = MessageType.SystemProblem,
        HttpResponseStatus successStatus = HttpResponseStatus.OK,
        HttpResponseStatus failureStatus = HttpResponseStatus.BadRequest,
        CancellationToken ct = default)
        => HandleAsync(_ => serviceFunc(), successMessage, failureFallbackMessage, successStatus, failureStatus, ct);
}
