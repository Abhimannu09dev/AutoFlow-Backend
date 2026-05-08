using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<ApiResponse<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            return ApiResponseFactory.Fail<AppointmentResponse>("CustomerId is required.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Date = request.Date,
            Time = request.Time,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Appointment created successfully.", Map(appointment));
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return ApiResponseFactory.Ok("Appointments retrieved successfully.", appointments.Select(Map).ToList());
    }

    public async Task<ApiResponse<AppointmentResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
            return ApiResponseFactory.FailNotFound<AppointmentResponse>("Appointment not found.");

        return ApiResponseFactory.Ok("Appointment retrieved successfully.", Map(appointment));
    }

    private static AppointmentResponse Map(Appointment a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        Date = a.Date,
        Time = a.Time,
        Status = a.Status,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}