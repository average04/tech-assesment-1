using AutoMapper;
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Queries;
using CustomerOnboarding.Application.Mapping;
using CustomerOnboarding.Domain.Customers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CustomerOnboarding.Tests.Customers;

public class GetCustomerByIdHandlerTests
{
    private readonly Mock<ICustomerRepository> _repo = new();
    private readonly IMapper _mapper;
    private readonly GetCustomerByIdHandler _sut;

    public GetCustomerByIdHandlerTests()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        _mapper = cfg.CreateMapper();
        _sut = new GetCustomerByIdHandler(_repo.Object, _mapper);
    }

    [Fact]
    public async Task Found_returns_dto()
    {
        var customer = new Customer("Ada", "Lovelace", "ada@example.com", "+15551234567", "data:image/png;base64,abc");
        _repo.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(customer);

        var result = await _sut.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task Not_found_returns_failure()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Customer?)null);

        var result = await _sut.Handle(new GetCustomerByIdQuery(id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.NotFound);
    }
}
