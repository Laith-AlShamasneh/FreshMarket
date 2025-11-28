namespace FreshMarket.Shared.Common;

public class ServiceResult<T>
{
    public HttpResponseStatus Code { get; init; }
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty; // Added Message here
    public string? ErrorCode { get; init; }
    public T? Data { get; init; }

    public static ServiceResult<T> Success(T data, string message, HttpResponseStatus code = HttpResponseStatus.OK) =>
        new()
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            Code = code
        };

    public static ServiceResult<T> Failure(string errorCode, string message, HttpResponseStatus code = HttpResponseStatus.BadRequest) =>
        new()
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            Message = message,
            Code = code
        };
}

public class ApiResponse<T>
{
    public HttpResponseStatus Code { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public Dictionary<string, string>? ValidationErrors { get; set; }
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message, HttpResponseStatus status) =>
        new()
        {
            Code = status,
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(string errorCode, string message, HttpResponseStatus status, Dictionary<string, string>? validationErrors = null) =>
        new()
        {
            Code = status,
            Success = false,
            ErrorCode = errorCode,
            Message = message,
            ValidationErrors = validationErrors
        };
}