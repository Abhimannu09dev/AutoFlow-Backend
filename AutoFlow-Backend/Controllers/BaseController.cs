using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoFlow_Backend.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected Guid? GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value is { } idStr
        && Guid.TryParse(idStr, out var userId) ? userId : null;

    protected bool IsStaffOrAdmin() =>
        User.IsInRole("Admin") || User.IsInRole("Staff");
}