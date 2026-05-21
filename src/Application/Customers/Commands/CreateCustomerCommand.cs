using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Commands;

public record CreateCustomerCommand(CreateCustomerRequest Request)
    : IRequest<Result<CustomerDto>>;
