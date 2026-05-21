using AutoMapper;
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using CustomerOnboarding.Domain.Customers;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Queries;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ICustomerRepository _repo;
    private readonly IMapper _mapper;

    public GetCustomerByIdHandler(ICustomerRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await _repo.GetByIdAsync(query.Id, ct);
        if (customer is null)
        {
            return Result<CustomerDto>.Failure(ErrorType.NotFound, $"Customer '{query.Id}' not found.");
        }
        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }
}
