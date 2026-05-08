namespace AutoFlow_Backend.Application.Common;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Ok<T>(string message, T data) =>
        new() { IsSuccess = true, Message = message, Data = data, ErrorType = ErrorType.None };

    public static ApiResponse<T> Fail<T>(string message, ErrorType errorType = ErrorType.ValidationError) =>
        new() { IsSuccess = false, Message = message, Data = default, ErrorType = errorType };

    public static ApiResponse<T> FailFromValidation<T>(List<string> errors) =>
        Fail<T>(errors.Count > 0 ? string.Join(" ", errors) : "Validation failed.", ErrorType.ValidationError);

    public static ApiResponse<T> FailNotFound<T>(string message) =>
        Fail<T>(message, ErrorType.NotFound);

    public static ApiResponse<T> FailConflict<T>(string message) =>
        Fail<T>(message, ErrorType.Conflict);
}