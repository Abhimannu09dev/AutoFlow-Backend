using AutoFlow_Backend.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Extensions;

public static class ApiResponseExtensions
{
    public static ActionResult<ApiResponse<T>> ToActionResult<T>(this ApiResponse<T> response)
    {
        return response.IsSuccess
            ? response
            : response.ErrorType switch
            {
                ErrorType.NotFound => new NotFoundObjectResult(response),
                ErrorType.Conflict => new ConflictObjectResult(response),
                _ => new BadRequestObjectResult(response)
            };
    }
}