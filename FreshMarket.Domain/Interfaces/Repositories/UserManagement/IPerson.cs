using FreshMarket.Domain.Entities.UserManagement;

namespace FreshMarket.Domain.Interfaces.Repositories.UserManagement;

public interface IPerson : IRepository<Person>
{
    Task<Person?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Person?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
}