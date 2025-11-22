namespace FreshMarket.Shared.Common;

public class ApiResponse<T>
{
    public HttpResponseStatus Code { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public T? Data { get; set; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static ServiceResult<T> Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}
