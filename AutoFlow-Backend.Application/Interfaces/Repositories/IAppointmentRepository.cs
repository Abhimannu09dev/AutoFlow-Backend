using AutoFlow_Backend.Domain.Entities;

namespace AutoFlow_Backend.Application.Interfaces.Repositories;

public interface IAppointmentRepository : IRepositoryBase<Appointment>
{
    Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Appointment>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
}