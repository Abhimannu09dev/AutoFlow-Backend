using AutoFlow_Backend.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AutoFlow_Backend.Filters;

public class ApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is not ObjectResult objectResult)
            return;

        // Only handle ApiResponse<T> results
        var resultType = objectResult.Value?.GetType();
        if (resultType is null || !IsApiResponse(resultType))
            return;

        // Use reflection to read the Status property
        var statusProp = resultType.GetProperty("Status");
        if (statusProp is null)
            return;

        var status = (bool)(statusProp.GetValue(objectResult.Value) ?? false);

        // Map Status true/false to proper HTTP status codes
        objectResult.StatusCode = status
            ? objectResult.StatusCode ?? StatusCodes.Status200OK
            : objectResult.StatusCode is StatusCodes.Status404NotFound
                ? StatusCodes.Status404NotFound
                : StatusCodes.Status400BadRequest;
    }

    private static bool IsApiResponse(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
}
