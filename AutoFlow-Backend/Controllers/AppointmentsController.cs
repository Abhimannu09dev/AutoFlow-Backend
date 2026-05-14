using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Extensions;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize(Roles = "Customer,Admin,Staff")]
[Tags("Appointments")]
public class AppointmentsController : BaseController
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// [Customer, Staff, Admin] Create a new appointment. Customers create for themselves; Staff/Admin can create for any customer.
    /// </summary>
    /// <param name="request">Appointment details (CustomerId optional for customers, Date, Time, Description)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created appointment details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _appointmentService.CreateAsync(request, userId, isStaffOrAdmin, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Customer, Staff, Admin] Get appointments. Customers see only their own; Staff/Admin see all.
    /// </summary>
    /// <param name="request">Pagination (page, pageSize) and sort parameters (sortBy, sortDir). Defaults: page=1, pageSize=20 (max 100), sortBy=date, sortDir=desc.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of appointments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<AppointmentResponse>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AppointmentResponse>>>> GetAll(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _appointmentService.GetAllAsync(request, userId, isStaffOrAdmin, cancellationToken);
        return response.ToActionResult();
    }

    /// <summary>
    /// [Customer, Staff, Admin] Get an appointment by ID. Customers can only access their own appointments; Staff/Admin can access any.
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Appointment details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var isStaffOrAdmin = IsStaffOrAdmin();

        var response = await _appointmentService.GetByIdAsync(id, userId, isStaffOrAdmin, cancellationToken);
        return response.ToActionResult();
    }
}