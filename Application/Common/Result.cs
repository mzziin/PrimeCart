namespace Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }
    public T? Value { get; init; }
    
    public static Result<T> Success(T value, int statusCode = 200) => new Result<T>
    {
        IsSuccess = true,
        Value = value,
        StatusCode = statusCode,
        Error = null
    };
    public static Result<T> Failure(string error, int statusCode = 500) => new Result<T>
    {
        IsSuccess = false,
        Error = error,
        StatusCode = statusCode,
        Value = default
    };
}