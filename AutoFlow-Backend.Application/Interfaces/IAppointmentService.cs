using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IAppointmentService
{
    Task<ApiResponse<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResponse<AppointmentResponse>>> GetAllAsync(
        PagedRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<AppointmentResponse>> GetByIdAsync(
        Guid id,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default);
}
