namespace AutoFlow_Backend.Application.Common;

public static class ApiResponseFactory
{
    public static ApiResponse<T> Ok<T>(string message, T data) =>
        new() { Status = true, Message = message, Data = data };

    public static ApiResponse<T> Fail<T>(string message) =>
        new() { Status = false, Message = message, Data = default };
    public static ApiResponse<T> FailFromValidation<T>(List<string> errors) =>
    Fail<T>(errors.Count > 0 ? string.Join(" ", errors) : "Validation failed.");
}
