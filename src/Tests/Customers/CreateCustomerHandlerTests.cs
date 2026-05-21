using AutoMapper;
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Commands;
using CustomerOnboarding.Application.Customers.Dtos;
using CustomerOnboarding.Application.Mapping;
using CustomerOnboarding.Domain.Common;
using CustomerOnboarding.Domain.Customers;
using FluentAssertions;
using Moq;
using Xunit;

namespace CustomerOnboarding.Tests.Customers;

public class CreateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly IMapper _mapper;
    private readonly CreateCustomerHandler _sut;

    public CreateCustomerHandlerTests()
    {
        var cfg = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        _mapper = cfg.CreateMapper();
        _sut = new CreateCustomerHandler(_repo.Object, _uow.Object, _mapper);
    }

    private static CreateCustomerCommand Command() => new(new CreateCustomerRequest(
        "Ada", "Lovelace", "ada@example.com", "+15551234567",
        "data:image/png;base64,iVBORw0KGgo="));

    [Fact]
    public async Task Happy_path_saves_and_returns_dto()
    {
        _repo.Setup(r => r.ExistsByEmailAsync("ada@example.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("ada@example.com");
        result.Value!.FirstName.Should().Be("Ada");
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Duplicate_email_returns_conflict_and_does_not_save()
    {
        _repo.Setup(r => r.ExistsByEmailAsync("ada@example.com", It.IsAny<CancellationToken>()))
             .ReturnsAsync(true);

        var result = await _sut.Handle(Command(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.Conflict);
        _repo.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Repo_throws_propagates()
    {
        _repo.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(false);
        _repo.Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("boom"));

        var act = async () => await _sut.Handle(Command(), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
    }
}
