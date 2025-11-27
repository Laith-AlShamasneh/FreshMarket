namespace FreshMarket.Shared.Common;

public class ServiceResult<T>
{
    public HttpResponseStatus Code { get; set; }
    public bool IsSuccess { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public T? Data { get; init; }

    public static ServiceResult<T> Success(HttpResponseStatus code, T data) =>
        new() { Code = code, IsSuccess = true, Data = data };

    public static ServiceResult<T> Failure(HttpResponseStatus code, string errorCode, string errorMessage) =>
        new() { Code = code, IsSuccess = false, ErrorCode = errorCode, ErrorMessage = errorMessage };
}

public class ApiResponse<T>
{
    public HttpResponseStatus Code { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string>? ValidationErrors { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(
        T data,
        MessageType messageType,
        Lang lang,
        HttpResponseStatus status = HttpResponseStatus.OK) =>
        new()
        {
            Code = status,
            Success = true,
            Message = Messages.Get(messageType, lang),
            Data = data
        };

    public static ApiResponse<T> Fail(
        string errorCode,
        MessageType messageType,
        Lang lang,
        HttpResponseStatus status = HttpResponseStatus.BadRequest,
        Dictionary<string, string>? validationErrors = null) =>
        new()
        {
            Code = status,
            Success = false,
            ErrorCode = errorCode,
            Message = Messages.Get(messageType, lang),
            ValidationErrors = validationErrors
        };
}