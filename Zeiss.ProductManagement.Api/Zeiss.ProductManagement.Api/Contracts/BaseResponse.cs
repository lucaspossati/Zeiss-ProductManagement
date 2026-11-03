namespace Zeiss.ProductManagement.Api.Contracts;

public sealed class BaseResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ApiValidationError> Errors { get; set; } = new();

    public static BaseResponse<T> Ok(T data, string message = "Operation completed successfully.") => new()
    {
        Success = true,
        Message = message,
        Data = data,
        Errors = new List<ApiValidationError>()
    };

    public static BaseResponse<T> Fail(string message, params ApiValidationError[] errors) => new()
    {
        Success = false,
        Message = message,
        Data = default,
        Errors = errors.ToList()
    };
}
