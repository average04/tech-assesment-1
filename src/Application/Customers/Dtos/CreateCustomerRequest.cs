namespace CustomerOnboarding.Application.Customers.Dtos;

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string SignatureBase64);
