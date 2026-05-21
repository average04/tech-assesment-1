using AutoMapper;
using CustomerOnboarding.Application.Customers.Queries;
using CustomerOnboarding.Application.Mapping;
using CustomerOnboarding.Domain.Customers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CustomerOnboarding.Tests.Customers;

public class ListCustomersHandlerTests
{
    private readonly Mock<ICustomerRepository> _repo = new();
    private readonly IMapper _mapper;
    private readonly ListCustomersHandler _sut;

    public ListCustomersHandlerTests()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        _mapper = cfg.CreateMapper();
        _sut = new ListCustomersHandler(_repo.Object, _mapper);
    }

    [Fact]
    public async Task Empty_returns_empty_list()
    {
        _repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(Array.Empty<Customer>());

        var result = await _sut.Handle(new ListCustomersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Multiple_returns_all_mapped()
    {
        var c1 = new Customer("Ada", "Lovelace", "ada@example.com", "+15551111111", "data:image/png;base64,abc");
        var c2 = new Customer("Grace", "Hopper", "grace@example.com", "+15552222222", "data:image/png;base64,abc");
        _repo.Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
             .ReturnsAsync(new[] { c1, c2 });

        var result = await _sut.Handle(new ListCustomersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
        result.Value!.Select(x => x.Email).Should().BeEquivalentTo(new[] { "ada@example.com", "grace@example.com" });
    }
}
