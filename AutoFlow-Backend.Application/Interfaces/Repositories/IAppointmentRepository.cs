using AutoFlow_Backend.Application.Common;
using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IAppointmentRepository : IRepositoryBase<Appointment>
{
    Task<PagedResponse<Appointment>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<Appointment>> GetPagedByCustomerIdAsync(Guid customerId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}