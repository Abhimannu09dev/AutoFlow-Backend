using AutoFlow_Backend.Application.DTOs.Vehicles;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Mappers;

public static class VehicleMapper
{
    public static VehicleResponseDto ToResponse(this Vehicle vehicle, Customer? customer = null)
    {
        return new VehicleResponseDto
        {
            Id = vehicle.Id,
            VehicleNumber = vehicle.VehicleNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            Color = vehicle.Color,
            VIN = vehicle.VIN,
            UserId = vehicle.UserId,
            CustomerId = customer?.Id,
            OwnerName = customer?.FullName,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt
        };
    }
}