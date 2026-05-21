namespace CustomerOnboarding.Domain.Customers;

public class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string SignatureBase64 { get; private set; } = string.Empty;
    public DateTime DateCreated { get; private set; }

    private Customer() { } // EF Core

    public Customer(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string signatureBase64)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        SignatureBase64 = signatureBase64;
        DateCreated = DateTime.UtcNow;
    }
}
