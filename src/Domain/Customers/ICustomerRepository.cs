namespace CustomerOnboarding.Domain.Customers;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken ct);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Customer>> ListAsync(CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
}
