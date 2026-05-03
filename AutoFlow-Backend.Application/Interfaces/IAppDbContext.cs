using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<Part> Parts { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Staff> Staffs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
