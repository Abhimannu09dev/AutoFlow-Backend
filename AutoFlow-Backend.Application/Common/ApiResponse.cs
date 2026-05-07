using AutoFlow_Backend.Application.Common;

public class ApiResponse<T>
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public ErrorType ErrorType { get; set; } = ErrorType.None;
}