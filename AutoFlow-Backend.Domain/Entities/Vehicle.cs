using System;
using System.Collections.Generic;
using System.Text;

namespace AutoFlow_Backend.Domain.Entities
{
    public class Vehicle
    {
        public Guid Id { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Mileage { get; set; } = 0;
        public int Year { get; set; }
        public string? Color { get; set; }
        public string? VIN { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
