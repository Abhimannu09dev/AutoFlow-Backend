using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class FailurePredictionService : IFailurePredictionService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IEnumerable<IFailurePredictionRule> _rules;

    public FailurePredictionService(
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository,
        IEnumerable<IFailurePredictionRule> rules)
    {
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
        _rules = rules;
    }

    public async Task<ApiResponse<List<FailurePredictionResponse>>> GetPredictionsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return ApiResponseFactory.FailNotFound<List<FailurePredictionResponse>>("Customer not found.");

        if (customer.ApplicationUserId is null)
            return ApiResponseFactory.Fail<List<FailurePredictionResponse>>("Customer does not have a linked account.");

        var vehicles = await _vehicleRepository.GetByUserIdAsync(customer.ApplicationUserId.Value, cancellationToken);
        if (vehicles.Count == 0)
            return ApiResponseFactory.Ok("No vehicles found for this customer.", new List<FailurePredictionResponse>());

        var predictions = vehicles.Select(BuildPrediction).ToList();

        return ApiResponseFactory.Ok("Failure predictions retrieved successfully.", predictions);
    }

    private FailurePredictionResponse BuildPrediction(Vehicle vehicle)
    {
        var vehicleAge = DateTime.UtcNow.Year - vehicle.Year;
        var failures = _rules
            .Where(rule => rule.Applies(vehicle, vehicleAge))
            .SelectMany(rule => rule.GetFailures(vehicle))
            .OrderByDescending(f => f.Severity == "High" ? 3 : f.Severity == "Medium" ? 2 : 1)
            .ToList();

        return new FailurePredictionResponse
        {
            VehicleId = vehicle.Id,
            VehicleNumber = vehicle.VehicleNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            PredictedFailures = failures
        };
    }
}
