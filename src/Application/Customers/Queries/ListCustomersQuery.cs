using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Queries;

public record ListCustomersQuery() : IRequest<Result<IReadOnlyList<CustomerDto>>>;
