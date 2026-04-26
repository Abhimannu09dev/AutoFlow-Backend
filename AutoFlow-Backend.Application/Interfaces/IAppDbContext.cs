using AutoFlow_Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoFlow_Backend.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Vendor> Vendors { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
