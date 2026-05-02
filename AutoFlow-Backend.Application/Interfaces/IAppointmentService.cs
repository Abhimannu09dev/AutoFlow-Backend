using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IAppointmentService
{
    Task<ApiResponse<AppointmentResponse>> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<AppointmentResponse>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<AppointmentResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
