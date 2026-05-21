namespace CustomerOnboarding.Application.Customers.Dtos;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string SignatureBase64,
    DateTime DateCreated);
