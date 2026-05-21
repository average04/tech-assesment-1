using AutoMapper;
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using CustomerOnboarding.Domain.Common;
using CustomerOnboarding.Domain.Customers;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Commands;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateCustomerHandler(ICustomerRepository repo, IUnitOfWork uow, IMapper mapper)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand command, CancellationToken ct)
    {
        var req = command.Request;

        if (await _repo.ExistsByEmailAsync(req.Email, ct))
        {
            return Result<CustomerDto>.Failure(
                ErrorType.Conflict,
                $"A customer with email '{req.Email}' already exists.");
        }

        var customer = new Customer(
            req.FirstName,
            req.LastName,
            req.Email,
            req.PhoneNumber,
            req.SignatureBase64);

        await _repo.AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }
}
