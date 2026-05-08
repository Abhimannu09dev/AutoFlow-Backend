using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Application.Interfaces;
using AutoFlow_Backend.Application.Interfaces.Repositories;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Services;

public class FailurePredictionService : IFailurePredictionService
{
    private const int BrakePadMileageThreshold = 50_000;
    private const int TimingBeltMileageThreshold = 80_000;
    private const int TransmissionFluidMileageThreshold = 100_000;
    private const int CoolantAgeThresholdYears = 5;
    private const int BatteryAgeThresholdYears = 10;

    private readonly ICustomerRepository _customerRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public FailurePredictionService(
        ICustomerRepository customerRepository,
        IVehicleRepository vehicleRepository)
    {
        _customerRepository = customerRepository;
        _vehicleRepository = vehicleRepository;
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

        var currentYear = DateTime.UtcNow.Year;
        var predictions = vehicles.Select(vehicle => BuildPrediction(vehicle, currentYear)).ToList();

        return ApiResponseFactory.Ok("Failure predictions retrieved successfully.", predictions);
    }

    private static FailurePredictionResponse BuildPrediction(Vehicle vehicle, int currentYear)
    {
        var failures = new List<PredictedFailureResponse>();
        var vehicleAge = currentYear - vehicle.Year;

        if (vehicle.Mileage > BrakePadMileageThreshold)
        {
            failures.Add(Failure("Brake Pads",
                $"Vehicle has {vehicle.Mileage:N0} km. Brake pads typically need replacement after {BrakePadMileageThreshold:N0} km.",
                "High"));
            failures.Add(Failure("Air Filter",
                $"Vehicle has {vehicle.Mileage:N0} km. Air filter replacement overdue.",
                "Low"));
            failures.Add(Failure("Oil Filter",
                $"Vehicle has {vehicle.Mileage:N0} km. Oil filter should be replaced regularly.",
                "Low"));
        }

        if (vehicle.Mileage > TimingBeltMileageThreshold)
        {
            failures.Add(Failure("Timing Belt",
                $"Vehicle has {vehicle.Mileage:N0} km. Timing belt replacement recommended after {TimingBeltMileageThreshold:N0} km.",
                "High"));
            failures.Add(Failure("Water Pump",
                $"Vehicle has {vehicle.Mileage:N0} km. Water pump typically fails around {TimingBeltMileageThreshold:N0}-{TransmissionFluidMileageThreshold:N0} km.",
                "High"));
            failures.Add(Failure("Spark Plugs",
                $"Vehicle has {vehicle.Mileage:N0} km. Spark plugs due for replacement.",
                "Medium"));
        }

        if (vehicle.Mileage > TransmissionFluidMileageThreshold)
        {
            failures.Add(Failure("Transmission Fluid",
                $"Vehicle has {vehicle.Mileage:N0} km. Transmission fluid change critical after {TransmissionFluidMileageThreshold:N0} km.",
                "High"));
            failures.Add(Failure("Shock Absorbers",
                $"Vehicle has {vehicle.Mileage:N0} km. Shock absorbers likely worn at this mileage.",
                "Medium"));
        }

        if (vehicleAge > CoolantAgeThresholdYears)
        {
            failures.Add(Failure("Coolant",
                $"Vehicle is {vehicleAge} years old. Coolant flush recommended every {CoolantAgeThresholdYears} years.",
                "Medium"));
        }

        if (vehicleAge > BatteryAgeThresholdYears)
        {
            failures.Add(Failure("Battery",
                $"Vehicle is {vehicleAge} years old. Batteries typically last 3-5 years.",
                "High"));
            failures.Add(Failure("Serpentine Belt",
                $"Vehicle is {vehicleAge} years old. Rubber belts degrade significantly after {BatteryAgeThresholdYears} years.",
                "Medium"));
        }

        return new FailurePredictionResponse
        {
            VehicleId = vehicle.Id,
            VehicleNumber = vehicle.VehicleNumber,
            Brand = vehicle.Brand,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            PredictedFailures = failures
                .OrderByDescending(f => f.Severity == "High" ? 3 : f.Severity == "Medium" ? 2 : 1)
                .ToList()
        };
    }

    private static PredictedFailureResponse Failure(string partName, string reason, string severity) =>
        new() { PartName = partName, Reason = reason, Severity = severity };
}