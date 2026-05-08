using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize(Roles = "Customer,Admin,Staff")]
[Tags("Appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    /// <summary>
    /// Create a new appointment
    /// </summary>
    /// <param name="request">Appointment details (CustomerId, Date, Time, Description)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created appointment details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.CreateAsync(request, cancellationToken);
        if (!response.IsSuccess)
            return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Get all appointments
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all appointments</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AppointmentResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get appointment by ID
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Appointment details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.GetByIdAsync(id, cancellationToken);
        if (!response.IsSuccess)
            return NotFound(response);
        return Ok(response);
    }
}