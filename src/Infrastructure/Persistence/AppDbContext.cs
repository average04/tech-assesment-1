using CustomerOnboarding.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace CustomerOnboarding.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var customer = modelBuilder.Entity<Customer>();
        customer.HasKey(c => c.Id);
        customer.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        customer.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        customer.Property(c => c.Email).IsRequired().HasMaxLength(254);
        customer.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(32);
        customer.Property(c => c.SignatureBase64).IsRequired();
        customer.Property(c => c.DateCreated).IsRequired();
        customer.HasIndex(c => c.Email).IsUnique();
    }
}
