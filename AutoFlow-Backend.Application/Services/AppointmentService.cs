using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Appointments;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Application.Mappers;
using AutoFlow_Backend.Domain.Entities;
using AutoFlow_Backend.Domain.Enums;

namespace AutoFlow_Backend.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleService _vehicleService;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        ICustomerRepository customerRepository,
        IVehicleService vehicleService)
    {
        _appointmentRepository = appointmentRepository;
        _customerRepository = customerRepository;
        _vehicleService = vehicleService;
    }

    public async Task<ApiResponse<AppointmentResponse>> CreateAsync(
        CreateAppointmentRequest request,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        Guid customerId;
        Customer? customer = null;

        if (isStaffOrAdmin && request.CustomerId.HasValue)
        {
            customerId = request.CustomerId.Value;
        }
        else if (requestingUserId.HasValue)
        {
            customer = await _customerRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
            if (customer is null)
                return ApiResponseFactory.Fail<AppointmentResponse>("Customer profile not found. Please contact support.");
            customerId = customer.Id;
        }
        else
        {
            return ApiResponseFactory.Fail<AppointmentResponse>("Unable to determine customer.");
        }

        if (request.VehicleId.HasValue)
        {
            var vehicleResult = await _vehicleService.GetByIdAsync(
                request.VehicleId.Value,
                requestingUserId: null,
                isStaffOrAdmin: true,
                cancellationToken);

            if (!vehicleResult.IsSuccess || vehicleResult.Data is null)
                return ApiResponseFactory.Fail<AppointmentResponse>("Vehicle not found.");

            customer ??= await _customerRepository.GetByIdAsync(customerId, cancellationToken);

            if (customer is null || !customer.ApplicationUserId.HasValue)
                return ApiResponseFactory.Fail<AppointmentResponse>("Customer profile incomplete. Cannot associate vehicle.");

            if (vehicleResult.Data.UserId != customer.ApplicationUserId.Value)
                return ApiResponseFactory.Fail<AppointmentResponse>("Vehicle does not belong to this customer.");
        }

        var status = Enum.TryParse<AppointmentStatus>(request.Status, ignoreCase: true, out var parsed)
            ? parsed
            : AppointmentStatus.Pending;

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = request.VehicleId,
            Date = request.Date,
            Time = request.Time,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        return ApiResponseFactory.Ok("Appointment created successfully.", AppointmentMapper.ToResponse(appointment));
    }

    public async Task<ApiResponse<List<AppointmentResponse>>> GetAllAsync(
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        List<Appointment> appointments;

        if (isStaffOrAdmin)
        {
            appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        }
        else if (requestingUserId.HasValue)
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
            if (customer is null)
                return ApiResponseFactory.Fail<List<AppointmentResponse>>("Customer profile not found.");

            appointments = await _appointmentRepository.GetByCustomerIdAsync(customer.Id, cancellationToken);
        }
        else
        {
            return ApiResponseFactory.Fail<List<AppointmentResponse>>("Unable to determine user.");
        }

        return ApiResponseFactory.Ok("Appointments retrieved successfully.", appointments.Select(AppointmentMapper.ToResponse).ToList());
    }

    public async Task<ApiResponse<AppointmentResponse>> GetByIdAsync(
        Guid id,
        Guid? requestingUserId,
        bool isStaffOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
            return ApiResponseFactory.FailNotFound<AppointmentResponse>("Appointment not found.");

        if (!isStaffOrAdmin && requestingUserId.HasValue)
        {
            var customer = await _customerRepository.GetByApplicationUserIdAsync(requestingUserId.Value, cancellationToken);
            if (customer is null || appointment.CustomerId != customer.Id)
                return ApiResponseFactory.FailNotFound<AppointmentResponse>("Appointment not found.");
        }

        return ApiResponseFactory.Ok("Appointment retrieved successfully.", AppointmentMapper.ToResponse(appointment));
    }
}