using AutoMapper;
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using CustomerOnboarding.Domain.Customers;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Queries;

public class ListCustomersHandler : IRequestHandler<ListCustomersQuery, Result<IReadOnlyList<CustomerDto>>>
{
    private readonly ICustomerRepository _repo;
    private readonly IMapper _mapper;

    public ListCustomersHandler(ICustomerRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> Handle(ListCustomersQuery query, CancellationToken ct)
    {
        var customers = await _repo.ListAsync(ct);
        var dtos = customers.Select(c => _mapper.Map<CustomerDto>(c)).ToList();
        return Result<IReadOnlyList<CustomerDto>>.Success(dtos);
    }
}
