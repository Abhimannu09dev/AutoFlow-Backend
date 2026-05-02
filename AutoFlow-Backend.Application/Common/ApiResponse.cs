namespace AutoFlow_Backend.Application.Common;

public class APIResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public int StatusCode { get; set; }
    public List<string>? Errors { get; set; }
}

public class ApiResponse<T>
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}