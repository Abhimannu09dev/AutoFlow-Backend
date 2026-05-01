using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoFlow_Backend.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> Create(
        [FromBody] CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.CreateAsync(request, cancellationToken);
        if (!response.Status)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AppointmentResponse>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.GetAllAsync(cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppointmentResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _appointmentService.GetByIdAsync(id, cancellationToken);
        if (!response.Status)
            return NotFound(response);

        return Ok(response);
    }
}
