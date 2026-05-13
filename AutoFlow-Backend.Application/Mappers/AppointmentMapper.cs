using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class AppointmentMapper
{
    public static AppointmentResponse ToResponse(Appointment appointment)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id,
            CustomerId = appointment.CustomerId,
            VehicleId = appointment.VehicleId,
            VehicleNumber = appointment.Vehicle?.VehicleNumber,
            Date = appointment.Date,
            Time = appointment.Time,
            Status = appointment.Status,
            CreatedAt = appointment.CreatedAt,
            UpdatedAt = appointment.UpdatedAt
        };
    }
}