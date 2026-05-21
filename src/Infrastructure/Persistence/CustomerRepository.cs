using CustomerOnboarding.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace CustomerOnboarding.Infrastructure.Persistence;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Customer customer, CancellationToken ct) =>
        _db.Customers.AddAsync(customer, ct).AsTask();

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Customer>> ListAsync(CancellationToken ct) =>
        await _db.Customers.OrderByDescending(c => c.DateCreated).ToListAsync(ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        _db.Customers.AnyAsync(c => c.Email == email, ct);
}
