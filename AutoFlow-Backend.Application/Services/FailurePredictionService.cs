using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Application.DTOs.Predictions;
using AutoFlow_Backend.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Services;

public class FailurePredictionService : IFailurePredictionService
{
    private readonly IAppDbContext _context;

    public FailurePredictionService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<FailurePredictionResponse>>> GetPredictionsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);

        if (customer is null)
            return Fail<List<FailurePredictionResponse>>("Customer not found.");

        if (customer.ApplicationUserId is null)
            return Fail<List<FailurePredictionResponse>>("Customer does not have a linked account.");

        var vehicles = await _context.Vehicles
            .AsNoTracking()
            .Where(v => v.UserId == customer.ApplicationUserId.Value)
            .ToListAsync(cancellationToken);

        if (vehicles.Count == 0)
            return Success("No vehicles found for this customer.", new List<FailurePredictionResponse>());

        var predictions = new List<FailurePredictionResponse>();

        foreach (var vehicle in vehicles)
        {
            var failures = new List<PredictedFailureResponse>();
            var currentYear = DateTime.UtcNow.Year;
            var vehicleAge = currentYear - vehicle.Year;

            if (vehicle.Mileage > 80000)
            {
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Timing Belt",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Timing belt replacement recommended after 80,000 km.",
                    Severity = "High"
                });
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Water Pump",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Water pump typically fails around 80,000-100,000 km.",
                    Severity = "High"
                });
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Spark Plugs",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Spark plugs due for replacement.",
                    Severity = "Medium"
                });
            }

            if (vehicle.Mileage > 50000)
            {
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Brake Pads",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Brake pads typically need replacement after 50,000 km.",
                    Severity = "High"
                });
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Air Filter",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Air filter replacement overdue.",
                    Severity = "Low"
                });
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Oil Filter",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Oil filter should be replaced regularly.",
                    Severity = "Low"
                });
            }

            if (vehicleAge > 10)
            {
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Battery",
                    Reason = $"Vehicle is {vehicleAge} years old. Batteries typically last 3-5 years.",
                    Severity = "High"
                });
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Serpentine Belt",
                    Reason = $"Vehicle is {vehicleAge} years old. Rubber belts degrade significantly after 10 years.",
                    Severity = "Medium"
                });
            }

            if (vehicleAge > 5)
            {
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Coolant",
                    Reason = $"Vehicle is {vehicleAge} years old. Coolant flush recommended every 5 years.",
                    Severity = "Medium"
                });
            }

            if (vehicle.Mileage > 100000)
            {
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Transmission Fluid",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Transmission fluid change critical after 100,000 km.",
                    Severity = "High"
                });
                failures.Add(new PredictedFailureResponse
                {
                    PartName = "Shock Absorbers",
                    Reason = $"Vehicle has {vehicle.Mileage:N0} km. Shock absorbers likely worn at this mileage.",
                    Severity = "Medium"
                });
            }

            predictions.Add(new FailurePredictionResponse
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
            });
        }

        return Success("Failure predictions retrieved successfully.", predictions);
    }

    private static ApiResponse<T> Success<T>(string message, T data) =>
        new() { Status = true, Message = message, Data = data };

    private static ApiResponse<T> Fail<T>(string message) =>
        new() { Status = false, Message = message, Data = default };
}