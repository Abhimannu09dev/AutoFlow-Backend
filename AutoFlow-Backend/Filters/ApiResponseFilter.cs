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

        var resultType = objectResult.Value?.GetType();
        if (resultType is null || !IsApiResponse(resultType))
            return;

        var statusProp = resultType.GetProperty("Status");
        if (statusProp is null)
            return;

        var status = (bool)(statusProp.GetValue(objectResult.Value) ?? false);

        if (status)
        {
            objectResult.StatusCode ??= StatusCodes.Status200OK;
            return;
        }

        if (objectResult.StatusCode is null)
        {
            objectResult.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private static bool IsApiResponse(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>);
}
