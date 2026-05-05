using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<Part> Parts { get; }
    DbSet<Staff> Staffs { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleItems> SaleItems { get; }
    DbSet<PartRequest> PartRequests { get; }
    DbSet<Review> Reviews { get; }
    DbSet<PurchaseInvoice> PurchaseInvoices { get; }
    DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
