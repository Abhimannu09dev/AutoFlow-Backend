using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppDbContext _dbContext;

    public AppointmentService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            return Fail<AppointmentResponse>("CustomerId is required.");

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Date = request.Date,
            Time = request.Time,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Success("Appointment created successfully.", Map(appointment));
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .Select(a => Map(a))
            .ToListAsync(cancellationToken);

        return Success("Appointments retrieved successfully.", appointments);
    }

    public async Task<ApiResponse<AppointmentResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => Map(a))
            .FirstOrDefaultAsync(cancellationToken);

        if (appointment is null)
            return Fail<AppointmentResponse>("Appointment not found.");

        return Success("Appointment retrieved successfully.", appointment);
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

    private static ApiResponse<T> Success<T>(string message, T data) =>
        new() { Status = true, Message = message, Data = data };

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Status = false, Message = message, Data = default };
}
