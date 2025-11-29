public class Result
{
    public bool IsSuccess { get; set; }
    public List<string> Errors { get; set; } = [];
    public int StatusCode { get; set; }

    public static Result Success(int statusCode = StatusCodes.Status200OK) =>
        new() { IsSuccess = true, StatusCode = statusCode };

    public static Result Failure(string errorMessage, int statusCode = StatusCodes.Status400BadRequest) =>
        new() { IsSuccess = false, Errors = [errorMessage], StatusCode = statusCode };

    public static Result Failure(List<string> errors, int statusCode = StatusCodes.Status400BadRequest) =>
        new() { IsSuccess = false, Errors = errors, StatusCode = statusCode };
}

public class Result<T> : Result
{
    public T? Value { get; set; }

    public static Result<T> Success(T value, int statusCode = StatusCodes.Status200OK) =>
        new()
        {
            IsSuccess = true,
            Value = value,
            StatusCode = statusCode
        };

    public static new Result<T> Failure(string errorMessage, int statusCode = StatusCodes.Status400BadRequest) =>
        new()
        {
            IsSuccess = false,
            Errors = [errorMessage],
            StatusCode = statusCode
        };

    public static new Result<T> Failure(List<string> errors, int statusCode = StatusCodes.Status400BadRequest) =>
        new()
        {
            IsSuccess = false,
            Errors = errors,
            StatusCode = statusCode
        };
}