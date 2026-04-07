namespace HouseHoldButler.Services;

public enum ServiceErrorType
{
    None,
    NotFound,
    Conflict,
    ValidationError
}

public class ServiceResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Data { get; private init; }
    public string? ErrorMessage { get; private init; }
    public ServiceErrorType ErrorType { get; private init; }

    public static ServiceResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    public static ServiceResult<T> NotFound(string message = "Resource not found.") => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.NotFound
    };

    public static ServiceResult<T> Conflict(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.Conflict
    };

    public static ServiceResult<T> ValidationError(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message,
        ErrorType = ServiceErrorType.ValidationError
    };
}
