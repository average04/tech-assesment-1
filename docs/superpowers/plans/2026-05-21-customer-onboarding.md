# Customer Onboarding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a reference Customer Registration & Onboarding system per the 1-hour technical assessment — .NET 10 Web API with clean architecture, EF Core/SQLite persistence, and a Vite + React + TypeScript frontend with an HTML canvas signature pad.

**Architecture:** Five-project .NET solution (Api, Application, Domain, Infrastructure, Tests) with one-way dependencies and a framework-free Domain. MediatR CQRS handlers with FluentValidation pipeline behavior, AutoMapper DTO mapping, and a Result pattern in place of exceptions for control flow. Vite/React/TS frontend with a from-scratch canvas signature pad.

**Tech Stack:** .NET 10, EF Core 10 + SQLite, MediatR, FluentValidation, AutoMapper, Serilog, xUnit + Moq + FluentAssertions, Vite + React 18 + TypeScript.

**Spec:** [docs/superpowers/specs/2026-05-21-customer-onboarding-system-design.md](../specs/2026-05-21-customer-onboarding-system-design.md)

---

## File Structure (locked in)

```
/.gitignore
/CustomerOnboarding.sln
/src/Api/
    CustomerOnboarding.Api.csproj
    Program.cs
    Controllers/CustomersController.cs
    Middleware/ExceptionHandlingMiddleware.cs
    Extensions/ResultExtensions.cs
    appsettings.json
    appsettings.Development.json
/src/Application/
    CustomerOnboarding.Application.csproj
    Common/Result.cs
    Common/ErrorType.cs
    Common/ValidationBehavior.cs
    Common/DependencyInjection.cs
    Customers/Dtos/CustomerDto.cs
    Customers/Dtos/CreateCustomerRequest.cs
    Customers/Commands/CreateCustomerCommand.cs
    Customers/Commands/CreateCustomerHandler.cs
    Customers/Commands/CreateCustomerValidator.cs
    Customers/Queries/GetCustomerByIdQuery.cs
    Customers/Queries/GetCustomerByIdHandler.cs
    Customers/Queries/ListCustomersQuery.cs
    Customers/Queries/ListCustomersHandler.cs
    Mapping/MappingProfile.cs
/src/Domain/
    CustomerOnboarding.Domain.csproj
    Customers/Customer.cs
    Customers/ICustomerRepository.cs
    Common/IUnitOfWork.cs
/src/Infrastructure/
    CustomerOnboarding.Infrastructure.csproj
    Persistence/AppDbContext.cs
    Persistence/CustomerRepository.cs
    Persistence/UnitOfWork.cs
    DependencyInjection.cs
/src/Tests/
    CustomerOnboarding.Tests.csproj
    Customers/CreateCustomerValidatorTests.cs
    Customers/CreateCustomerHandlerTests.cs
    Customers/GetCustomerByIdHandlerTests.cs
    Customers/ListCustomersHandlerTests.cs
/frontend/
    package.json, tsconfig.json, vite.config.ts, index.html, .env
    src/main.tsx
    src/App.tsx
    src/App.css
    src/types/Customer.ts
    src/api/customerApi.ts
    src/components/SignaturePad.tsx
    src/components/CustomerForm.tsx
    src/components/ConfirmationCard.tsx
/README.md
```

---

## Task 1: Repository setup (.gitignore + solution)

**Files:**
- Create: `.gitignore`
- Create: `CustomerOnboarding.sln`

- [ ] **Step 1: Write .gitignore**

Create `.gitignore`:
```gitignore
# .NET
bin/
obj/
*.user
*.suo
.vs/
*.db
*.db-shm
*.db-wal
logs/

# Node
node_modules/
dist/
.vite/

# Editor
.idea/
.vscode/

# OS
Thumbs.db
.DS_Store
```

- [ ] **Step 2: Create solution**

Run:
```bash
cd d:/Projects/tech-assement-1
dotnet new sln -n CustomerOnboarding
```

Expected: `CustomerOnboarding.sln created.`

- [ ] **Step 3: Commit**

```bash
git add .gitignore CustomerOnboarding.sln
git commit -m "chore: initialize solution and .gitignore"
```

---

## Task 2: Create the five .NET projects and wire references

**Files:**
- Create: `src/Api/CustomerOnboarding.Api.csproj`
- Create: `src/Application/CustomerOnboarding.Application.csproj`
- Create: `src/Domain/CustomerOnboarding.Domain.csproj`
- Create: `src/Infrastructure/CustomerOnboarding.Infrastructure.csproj`
- Create: `src/Tests/CustomerOnboarding.Tests.csproj`

- [ ] **Step 1: Scaffold projects**

Run from repo root:
```bash
dotnet new webapi   -n CustomerOnboarding.Api           -o src/Api           --use-controllers --no-openapi
dotnet new classlib -n CustomerOnboarding.Application   -o src/Application
dotnet new classlib -n CustomerOnboarding.Domain        -o src/Domain
dotnet new classlib -n CustomerOnboarding.Infrastructure -o src/Infrastructure
dotnet new xunit    -n CustomerOnboarding.Tests         -o src/Tests
```

- [ ] **Step 2: Remove default Class1.cs stubs**

```bash
rm -f src/Application/Class1.cs src/Domain/Class1.cs src/Infrastructure/Class1.cs
```

- [ ] **Step 3: Add all projects to the solution**

```bash
dotnet sln add src/Api/CustomerOnboarding.Api.csproj
dotnet sln add src/Application/CustomerOnboarding.Application.csproj
dotnet sln add src/Domain/CustomerOnboarding.Domain.csproj
dotnet sln add src/Infrastructure/CustomerOnboarding.Infrastructure.csproj
dotnet sln add src/Tests/CustomerOnboarding.Tests.csproj
```

- [ ] **Step 4: Wire project references (one-way only)**

```bash
# Application -> Domain
dotnet add src/Application/CustomerOnboarding.Application.csproj reference src/Domain/CustomerOnboarding.Domain.csproj

# Infrastructure -> Application, Domain
dotnet add src/Infrastructure/CustomerOnboarding.Infrastructure.csproj reference src/Application/CustomerOnboarding.Application.csproj
dotnet add src/Infrastructure/CustomerOnboarding.Infrastructure.csproj reference src/Domain/CustomerOnboarding.Domain.csproj

# Api -> Application, Infrastructure
dotnet add src/Api/CustomerOnboarding.Api.csproj reference src/Application/CustomerOnboarding.Application.csproj
dotnet add src/Api/CustomerOnboarding.Api.csproj reference src/Infrastructure/CustomerOnboarding.Infrastructure.csproj

# Tests -> Application, Domain, Infrastructure
dotnet add src/Tests/CustomerOnboarding.Tests.csproj reference src/Application/CustomerOnboarding.Application.csproj
dotnet add src/Tests/CustomerOnboarding.Tests.csproj reference src/Domain/CustomerOnboarding.Domain.csproj
dotnet add src/Tests/CustomerOnboarding.Tests.csproj reference src/Infrastructure/CustomerOnboarding.Infrastructure.csproj
```

- [ ] **Step 5: Verify build**

```bash
dotnet build CustomerOnboarding.sln
```
Expected: `Build succeeded.` with 0 errors, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add CustomerOnboarding.sln src/
git commit -m "chore: scaffold 5-project solution with references"
```

---

## Task 3: Install NuGet packages

**Files:** All five `.csproj` files (modified by `dotnet add package`)

- [ ] **Step 1: Application packages**

```bash
dotnet add src/Application/CustomerOnboarding.Application.csproj package MediatR
dotnet add src/Application/CustomerOnboarding.Application.csproj package FluentValidation
dotnet add src/Application/CustomerOnboarding.Application.csproj package FluentValidation.DependencyInjectionExtensions
dotnet add src/Application/CustomerOnboarding.Application.csproj package AutoMapper
dotnet add src/Application/CustomerOnboarding.Application.csproj package AutoMapper.Extensions.Microsoft.DependencyInjection
dotnet add src/Application/CustomerOnboarding.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
```

- [ ] **Step 2: Infrastructure packages**

```bash
dotnet add src/Infrastructure/CustomerOnboarding.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/Infrastructure/CustomerOnboarding.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/Infrastructure/CustomerOnboarding.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
```

- [ ] **Step 3: Api packages**

```bash
dotnet add src/Api/CustomerOnboarding.Api.csproj package Serilog.AspNetCore
dotnet add src/Api/CustomerOnboarding.Api.csproj package Serilog.Sinks.File
dotnet add src/Api/CustomerOnboarding.Api.csproj package Swashbuckle.AspNetCore
```

- [ ] **Step 4: Tests packages**

```bash
dotnet add src/Tests/CustomerOnboarding.Tests.csproj package Moq
dotnet add src/Tests/CustomerOnboarding.Tests.csproj package FluentAssertions
dotnet add src/Tests/CustomerOnboarding.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory
```

- [ ] **Step 5: Verify build**

```bash
dotnet build CustomerOnboarding.sln
```
Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/
git commit -m "chore: add NuGet packages (MediatR, EF Core, FluentValidation, AutoMapper, Serilog, xUnit deps)"
```

---

## Task 4: Domain — Customer entity and interfaces

**Files:**
- Create: `src/Domain/Customers/Customer.cs`
- Create: `src/Domain/Customers/ICustomerRepository.cs`
- Create: `src/Domain/Common/IUnitOfWork.cs`

- [ ] **Step 1: Create Customer.cs**

```csharp
namespace CustomerOnboarding.Domain.Customers;

public class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string SignatureBase64 { get; private set; } = string.Empty;
    public DateTime DateCreated { get; private set; }

    private Customer() { } // EF Core

    public Customer(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string signatureBase64)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        SignatureBase64 = signatureBase64;
        DateCreated = DateTime.UtcNow;
    }
}
```

- [ ] **Step 2: Create ICustomerRepository.cs**

```csharp
namespace CustomerOnboarding.Domain.Customers;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken ct);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Customer>> ListAsync(CancellationToken ct);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
}
```

- [ ] **Step 3: Create IUnitOfWork.cs**

```csharp
namespace CustomerOnboarding.Domain.Common;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

- [ ] **Step 4: Verify build**

```bash
dotnet build src/Domain/CustomerOnboarding.Domain.csproj
```
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/Domain/
git commit -m "feat(domain): add Customer entity and repository/uow interfaces"
```

---

## Task 5: Application — Result pattern

**Files:**
- Create: `src/Application/Common/ErrorType.cs`
- Create: `src/Application/Common/Result.cs`

- [ ] **Step 1: Create ErrorType.cs**

```csharp
namespace CustomerOnboarding.Application.Common;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Unexpected
}
```

- [ ] **Step 2: Create Result.cs**

```csharp
namespace CustomerOnboarding.Application.Common;

public class Result
{
    public bool IsSuccess { get; }
    public IReadOnlyList<string> Errors { get; }
    public ErrorType ErrorType { get; }

    protected Result(bool isSuccess, IReadOnlyList<string> errors, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, Array.Empty<string>(), ErrorType.None);

    public static Result Failure(ErrorType type, params string[] errors) =>
        new(false, errors, type);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, IReadOnlyList<string> errors, ErrorType errorType)
        : base(isSuccess, errors, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) =>
        new(true, value, Array.Empty<string>(), ErrorType.None);

    public static new Result<T> Failure(ErrorType type, params string[] errors) =>
        new(false, default, errors, type);
}
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/Application/CustomerOnboarding.Application.csproj
```
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Application/Common/
git commit -m "feat(application): add Result<T> and ErrorType"
```

---

## Task 6: Application — DTOs and AutoMapper profile

**Files:**
- Create: `src/Application/Customers/Dtos/CustomerDto.cs`
- Create: `src/Application/Customers/Dtos/CreateCustomerRequest.cs`
- Create: `src/Application/Mapping/MappingProfile.cs`

- [ ] **Step 1: Create CustomerDto.cs**

```csharp
namespace CustomerOnboarding.Application.Customers.Dtos;

public record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string SignatureBase64,
    DateTime DateCreated);
```

- [ ] **Step 2: Create CreateCustomerRequest.cs**

```csharp
namespace CustomerOnboarding.Application.Customers.Dtos;

public record CreateCustomerRequest(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string SignatureBase64);
```

- [ ] **Step 3: Create MappingProfile.cs**

```csharp
using AutoMapper;
using CustomerOnboarding.Application.Customers.Dtos;
using CustomerOnboarding.Domain.Customers;

namespace CustomerOnboarding.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
    }
}
```

- [ ] **Step 4: Verify build**

```bash
dotnet build src/Application/CustomerOnboarding.Application.csproj
```
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/Application/Customers/Dtos/ src/Application/Mapping/
git commit -m "feat(application): add CustomerDto, CreateCustomerRequest, AutoMapper profile"
```

---

## Task 7: Application — CreateCustomerCommand + Validator (TDD)

**Files:**
- Create: `src/Application/Customers/Commands/CreateCustomerCommand.cs`
- Create: `src/Application/Customers/Commands/CreateCustomerValidator.cs`
- Create: `src/Tests/Customers/CreateCustomerValidatorTests.cs`

- [ ] **Step 1: Create CreateCustomerCommand.cs**

```csharp
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Commands;

public record CreateCustomerCommand(CreateCustomerRequest Request)
    : IRequest<Result<CustomerDto>>;
```

- [ ] **Step 2: Write the failing validator tests**

Create `src/Tests/Customers/CreateCustomerValidatorTests.cs`:
```csharp
using CustomerOnboarding.Application.Customers.Commands;
using CustomerOnboarding.Application.Customers.Dtos;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CustomerOnboarding.Tests.Customers;

public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _sut = new();

    private static CreateCustomerCommand Valid() => new(new CreateCustomerRequest(
        FirstName: "Ada",
        LastName: "Lovelace",
        Email: "ada@example.com",
        PhoneNumber: "+15551234567",
        SignatureBase64: "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="));

    [Fact]
    public void Valid_input_passes()
    {
        _sut.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void First_name_required(string firstName)
    {
        var cmd = Valid() with { Request = Valid().Request with { FirstName = firstName } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.FirstName");
    }

    [Fact]
    public void First_name_max_100()
    {
        var cmd = Valid() with { Request = Valid().Request with { FirstName = new string('a', 101) } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.FirstName");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing@tld")]
    public void Email_must_be_valid(string email)
    {
        var cmd = Valid() with { Request = Valid().Request with { Email = email } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("123")]
    public void Phone_must_be_valid(string phone)
    {
        var cmd = Valid() with { Request = Valid().Request with { PhoneNumber = phone } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.PhoneNumber");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-data-url")]
    [InlineData("data:image/jpeg;base64,abc")]
    public void Signature_must_be_png_data_url(string sig)
    {
        var cmd = Valid() with { Request = Valid().Request with { SignatureBase64 = sig } };
        _sut.TestValidate(cmd).ShouldHaveValidationErrorFor("Request.SignatureBase64");
    }
}
```

- [ ] **Step 3: Run tests — confirm they fail**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~CreateCustomerValidatorTests"
```
Expected: Build error — `CreateCustomerValidator` does not exist yet.

- [ ] **Step 4: Implement CreateCustomerValidator.cs**

```csharp
using FluentValidation;

namespace CustomerOnboarding.Application.Customers.Commands;

public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
{
    private const string PngDataUrlPrefix = "data:image/png;base64,";

    public CreateCustomerValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Request.PhoneNumber)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{7,15}$")
            .WithMessage("PhoneNumber must be 7-15 digits, optionally prefixed with '+'.");

        RuleFor(x => x.Request.SignatureBase64)
            .NotEmpty()
            .Must(BeValidPngDataUrl)
            .WithMessage("SignatureBase64 must be a base64-encoded PNG data URL.");
    }

    private static bool BeValidPngDataUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value) && value.StartsWith(PngDataUrlPrefix, StringComparison.Ordinal);
}
```

- [ ] **Step 5: Run tests — confirm they pass**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~CreateCustomerValidatorTests"
```
Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/Application/Customers/Commands/ src/Tests/Customers/CreateCustomerValidatorTests.cs
git commit -m "feat(application): CreateCustomerCommand + validator with tests"
```

---

## Task 8: Application — CreateCustomerHandler (TDD)

**Files:**
- Create: `src/Application/Customers/Commands/CreateCustomerHandler.cs`
- Create: `src/Tests/Customers/CreateCustomerHandlerTests.cs`

- [ ] **Step 1: Write the failing handler tests**

Create `src/Tests/Customers/CreateCustomerHandlerTests.cs`:
```csharp
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
```

- [ ] **Step 2: Run tests — confirm they fail**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~CreateCustomerHandlerTests"
```
Expected: Build error — `CreateCustomerHandler` does not exist yet.

- [ ] **Step 3: Implement CreateCustomerHandler.cs**

```csharp
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
```

- [ ] **Step 4: Run tests — confirm they pass**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~CreateCustomerHandlerTests"
```
Expected: All 3 tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Application/Customers/Commands/CreateCustomerHandler.cs src/Tests/Customers/CreateCustomerHandlerTests.cs
git commit -m "feat(application): CreateCustomerHandler with tests (happy/conflict/throws)"
```

---

## Task 9: Application — GetCustomerByIdHandler (TDD)

**Files:**
- Create: `src/Application/Customers/Queries/GetCustomerByIdQuery.cs`
- Create: `src/Application/Customers/Queries/GetCustomerByIdHandler.cs`
- Create: `src/Tests/Customers/GetCustomerByIdHandlerTests.cs`

- [ ] **Step 1: Create GetCustomerByIdQuery.cs**

```csharp
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Queries;

public record GetCustomerByIdQuery(Guid Id) : IRequest<Result<CustomerDto>>;
```

- [ ] **Step 2: Write failing tests**

Create `src/Tests/Customers/GetCustomerByIdHandlerTests.cs`:
```csharp
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
```

- [ ] **Step 3: Run tests — confirm they fail**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~GetCustomerByIdHandlerTests"
```
Expected: Build error — `GetCustomerByIdHandler` does not exist.

- [ ] **Step 4: Implement GetCustomerByIdHandler.cs**

```csharp
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
```

- [ ] **Step 5: Run tests — confirm they pass**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~GetCustomerByIdHandlerTests"
```
Expected: Both tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/Application/Customers/Queries/ src/Tests/Customers/GetCustomerByIdHandlerTests.cs
git commit -m "feat(application): GetCustomerByIdHandler with tests (found/not-found)"
```

---

## Task 10: Application — ListCustomersHandler (TDD)

**Files:**
- Create: `src/Application/Customers/Queries/ListCustomersQuery.cs`
- Create: `src/Application/Customers/Queries/ListCustomersHandler.cs`
- Create: `src/Tests/Customers/ListCustomersHandlerTests.cs`

- [ ] **Step 1: Create ListCustomersQuery.cs**

```csharp
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Application.Customers.Dtos;
using MediatR;

namespace CustomerOnboarding.Application.Customers.Queries;

public record ListCustomersQuery() : IRequest<Result<IReadOnlyList<CustomerDto>>>;
```

- [ ] **Step 2: Write failing tests**

Create `src/Tests/Customers/ListCustomersHandlerTests.cs`:
```csharp
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
```

- [ ] **Step 3: Run tests — confirm they fail**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~ListCustomersHandlerTests"
```
Expected: Build error — `ListCustomersHandler` does not exist.

- [ ] **Step 4: Implement ListCustomersHandler.cs**

```csharp
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
```

- [ ] **Step 5: Run tests — confirm they pass**

```bash
dotnet test src/Tests/CustomerOnboarding.Tests.csproj --filter "FullyQualifiedName~ListCustomersHandlerTests"
```
Expected: Both tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/Application/Customers/Queries/ListCustomers* src/Tests/Customers/ListCustomersHandlerTests.cs
git commit -m "feat(application): ListCustomersHandler with tests"
```

---

## Task 11: Application — ValidationBehavior pipeline + DI registration

**Files:**
- Create: `src/Application/Common/ValidationBehavior.cs`
- Create: `src/Application/Common/DependencyInjection.cs`

- [ ] **Step 1: Create ValidationBehavior.cs**

```csharp
using System.Reflection;
using FluentValidation;
using MediatR;

namespace CustomerOnboarding.Application.Common;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var messages = failures.Select(f => f.ErrorMessage).ToArray();

        // TResponse must be Result or Result<T>. Build the failure via reflection.
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(
                nameof(Result<object>.Failure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                new[] { typeof(ErrorType), typeof(string[]) })!;
            return (TResponse)failureMethod.Invoke(null, new object[] { ErrorType.Validation, messages })!;
        }

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(ErrorType.Validation, messages);
        }

        // Validators registered for a request whose response isn't a Result — let it through.
        return await next();
    }
}
```

- [ ] **Step 2: Create DependencyInjection.cs**

```csharp
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CustomerOnboarding.Application.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/Application/CustomerOnboarding.Application.csproj
```
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Application/Common/ValidationBehavior.cs src/Application/Common/DependencyInjection.cs
git commit -m "feat(application): MediatR validation pipeline + DI extension"
```

---

## Task 12: Infrastructure — AppDbContext

**Files:**
- Create: `src/Infrastructure/Persistence/AppDbContext.cs`

- [ ] **Step 1: Create AppDbContext.cs**

```csharp
using CustomerOnboarding.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace CustomerOnboarding.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var customer = modelBuilder.Entity<Customer>();
        customer.HasKey(c => c.Id);
        customer.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
        customer.Property(c => c.LastName).IsRequired().HasMaxLength(100);
        customer.Property(c => c.Email).IsRequired().HasMaxLength(254);
        customer.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(32);
        customer.Property(c => c.SignatureBase64).IsRequired();
        customer.Property(c => c.DateCreated).IsRequired();
        customer.HasIndex(c => c.Email).IsUnique();
    }
}
```

- [ ] **Step 2: Verify build**

```bash
dotnet build src/Infrastructure/CustomerOnboarding.Infrastructure.csproj
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/Infrastructure/Persistence/AppDbContext.cs
git commit -m "feat(infrastructure): AppDbContext with Customer config + unique email index"
```

---

## Task 13: Infrastructure — Repository, UnitOfWork, DI

**Files:**
- Create: `src/Infrastructure/Persistence/CustomerRepository.cs`
- Create: `src/Infrastructure/Persistence/UnitOfWork.cs`
- Create: `src/Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: Create CustomerRepository.cs**

```csharp
using CustomerOnboarding.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace CustomerOnboarding.Infrastructure.Persistence;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;

    public CustomerRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task AddAsync(Customer customer, CancellationToken ct) =>
        _db.Customers.AddAsync(customer, ct).AsTask();

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Customer>> ListAsync(CancellationToken ct) =>
        await _db.Customers.OrderByDescending(c => c.DateCreated).ToListAsync(ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        _db.Customers.AnyAsync(c => c.Email == email, ct);
}
```

- [ ] **Step 2: Create UnitOfWork.cs**

```csharp
using CustomerOnboarding.Domain.Common;

namespace CustomerOnboarding.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
```

- [ ] **Step 3: Create DependencyInjection.cs**

```csharp
using CustomerOnboarding.Domain.Common;
using CustomerOnboarding.Domain.Customers;
using CustomerOnboarding.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerOnboarding.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("Default") ?? "Data Source=customers.db";

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(conn));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
```

- [ ] **Step 4: Verify build**

```bash
dotnet build src/Infrastructure/CustomerOnboarding.Infrastructure.csproj
```
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/Infrastructure/
git commit -m "feat(infrastructure): CustomerRepository, UnitOfWork, DI extension"
```

---

## Task 14: Api — ExceptionHandlingMiddleware + Result extension

**Files:**
- Create: `src/Api/Middleware/ExceptionHandlingMiddleware.cs`
- Create: `src/Api/Extensions/ResultExtensions.cs`

- [ ] **Step 1: Create ExceptionHandlingMiddleware.cs**

```csharp
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CustomerOnboarding.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
```

- [ ] **Step 2: Create ResultExtensions.cs**

```csharp
using CustomerOnboarding.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOnboarding.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return result.ErrorType switch
        {
            ErrorType.NotFound  => controller.NotFound(BuildProblem(controller, result, StatusCodes.Status404NotFound, "Not Found")),
            ErrorType.Conflict  => controller.Conflict(BuildProblem(controller, result, StatusCodes.Status409Conflict, "Conflict")),
            ErrorType.Validation => controller.BadRequest(BuildProblem(controller, result, StatusCodes.Status400BadRequest, "Validation Failed")),
            _                   => controller.StatusCode(500, BuildProblem(controller, result, StatusCodes.Status500InternalServerError, "Unexpected Error"))
        };
    }

    private static ProblemDetails BuildProblem<T>(ControllerBase controller, Result<T> result, int status, string title) =>
        new()
        {
            Status = status,
            Title = title,
            Detail = string.Join("; ", result.Errors),
            Instance = controller.HttpContext.Request.Path
        };
}
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/Api/CustomerOnboarding.Api.csproj
```
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Api/Middleware/ src/Api/Extensions/
git commit -m "feat(api): ExceptionHandlingMiddleware + Result->IActionResult extension"
```

---

## Task 15: Api — CustomersController

**Files:**
- Create: `src/Api/Controllers/CustomersController.cs`
- Delete: `src/Api/Controllers/WeatherForecastController.cs` (if present)
- Delete: `src/Api/WeatherForecast.cs` (if present)

- [ ] **Step 1: Remove default template files (if present)**

```bash
rm -f src/Api/Controllers/WeatherForecastController.cs src/Api/WeatherForecast.cs
```

- [ ] **Step 2: Create CustomersController.cs**

```csharp
using CustomerOnboarding.Api.Extensions;
using CustomerOnboarding.Application.Customers.Commands;
using CustomerOnboarding.Application.Customers.Dtos;
using CustomerOnboarding.Application.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOnboarding.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCustomerCommand(request), ct);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }
        return result.ToActionResult(this);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id), ct);
        return result.ToActionResult(this);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListCustomersQuery(), ct);
        return result.ToActionResult(this);
    }
}
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/Api/CustomerOnboarding.Api.csproj
```
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Api/Controllers/CustomersController.cs
git commit -m "feat(api): CustomersController (POST, GET by id, GET list)"
```

---

## Task 16: Api — Program.cs (DI, Serilog, CORS, Swagger, EnsureCreated)

**Files:**
- Modify: `src/Api/Program.cs` (full rewrite)
- Modify: `src/Api/appsettings.json`
- Modify: `src/Api/appsettings.Development.json`

- [ ] **Step 1: Replace Program.cs**

Overwrite `src/Api/Program.cs`:
```csharp
using CustomerOnboarding.Api.Middleware;
using CustomerOnboarding.Application.Common;
using CustomerOnboarding.Infrastructure;
using CustomerOnboarding.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string CorsPolicy = "Frontend";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors(CorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
```

- [ ] **Step 2: Set appsettings.json**

Overwrite `src/Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Data Source=customers.db"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
      }
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 3: Set appsettings.Development.json**

Overwrite `src/Api/appsettings.Development.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

- [ ] **Step 4: Configure URLs in launchSettings.json**

Overwrite `src/Api/Properties/launchSettings.json`:
```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

- [ ] **Step 5: Build the whole solution**

```bash
dotnet build CustomerOnboarding.sln
```
Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/Api/Program.cs src/Api/appsettings*.json src/Api/Properties/launchSettings.json
git commit -m "feat(api): wire DI, Serilog, CORS, Swagger, EnsureCreated in Program.cs"
```

---

## Task 17: Run all tests + manual API smoke test

- [ ] **Step 1: Run the full test suite**

```bash
dotnet test CustomerOnboarding.sln
```
Expected: All tests pass (validator + 3 handlers).

- [ ] **Step 2: Start the API**

```bash
dotnet run --project src/Api/CustomerOnboarding.Api.csproj
```
Expected: `Now listening on: http://localhost:5000`. SQLite file `customers.db` appears next to the Api binary.

- [ ] **Step 3: Smoke test endpoints**

In a second terminal:
```bash
# List (empty)
curl -s http://localhost:5000/api/customers

# Create
curl -s -X POST http://localhost:5000/api/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Ada","lastName":"Lovelace","email":"ada@example.com","phoneNumber":"+15551234567","signatureBase64":"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII="}'

# Bad input (missing email)
curl -s -X POST http://localhost:5000/api/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Ada","lastName":"Lovelace","email":"","phoneNumber":"+15551234567","signatureBase64":"data:image/png;base64,abc"}'
```

Expected:
- First curl → `[]`
- Second curl → 201 with full DTO
- Third curl → 400 ProblemDetails

- [ ] **Step 4: Stop the API (Ctrl+C) and commit any incidental changes**

If everything passed: no commit needed.

---

## Task 18: Frontend — Vite scaffold + dependencies

**Files:**
- Create: `frontend/` (whole directory via `npm create vite`)

- [ ] **Step 1: Scaffold Vite + React + TS**

```bash
cd d:/Projects/tech-assement-1
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install
```

- [ ] **Step 2: Remove the default boilerplate**

```bash
rm -rf src/assets
rm -f src/App.css src/App.tsx src/index.css public/vite.svg
```
(We will recreate `App.tsx` and `App.css` from scratch in later tasks.)

- [ ] **Step 3: Configure Vite proxy and port**

Overwrite `frontend/vite.config.ts`:
```ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
});
```

- [ ] **Step 4: Set env file**

Create `frontend/.env`:
```
VITE_API_BASE_URL=/api
```

- [ ] **Step 5: Commit (build verification deferred to Task 22, after App.tsx is recreated)**

```bash
cd ..
git add frontend/
git commit -m "chore(frontend): scaffold Vite + React + TS, configure proxy"
```

---

## Task 19: Frontend — types + API client

**Files:**
- Create: `frontend/src/types/Customer.ts`
- Create: `frontend/src/api/customerApi.ts`

- [ ] **Step 1: Create Customer.ts**

```typescript
export interface CustomerDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  signatureBase64: string;
  dateCreated: string;
}

export interface CreateCustomerRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  signatureBase64: string;
}

export interface ApiError {
  status: number;
  title: string;
  detail: string;
}
```

- [ ] **Step 2: Create customerApi.ts**

```typescript
import type { CreateCustomerRequest, CustomerDto, ApiError } from '../types/Customer';

const BASE = import.meta.env.VITE_API_BASE_URL ?? '/api';

async function handle<T>(res: Response): Promise<T> {
  if (res.ok) return (await res.json()) as T;
  let detail = res.statusText;
  let title = 'Request failed';
  try {
    const body = await res.json();
    title = body.title ?? title;
    detail = body.detail ?? detail;
  } catch { /* not JSON */ }
  const err: ApiError = { status: res.status, title, detail };
  throw err;
}

export async function createCustomer(req: CreateCustomerRequest): Promise<CustomerDto> {
  const res = await fetch(`${BASE}/customers`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  });
  return handle<CustomerDto>(res);
}

export async function listCustomers(): Promise<CustomerDto[]> {
  return handle<CustomerDto[]>(await fetch(`${BASE}/customers`));
}

export async function getCustomer(id: string): Promise<CustomerDto> {
  return handle<CustomerDto>(await fetch(`${BASE}/customers/${id}`));
}
```

- [ ] **Step 3: Commit**

```bash
git add frontend/src/types/ frontend/src/api/
git commit -m "feat(frontend): API types and customerApi client"
```

---

## Task 20: Frontend — SignaturePad (raw canvas)

**Files:**
- Create: `frontend/src/components/SignaturePad.tsx`

- [ ] **Step 1: Create SignaturePad.tsx**

```tsx
import { forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react';

export interface SignaturePadHandle {
  getDataUrl: () => string;
  clear: () => void;
  isEmpty: () => boolean;
}

interface Props {
  width?: number;
  height?: number;
}

const SignaturePad = forwardRef<SignaturePadHandle, Props>(({ width = 480, height = 160 }, ref) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const drawingRef = useRef(false);
  const [empty, setEmpty] = useState(true);

  useEffect(() => {
    const canvas = canvasRef.current!;
    const dpr = window.devicePixelRatio || 1;
    canvas.width = width * dpr;
    canvas.height = height * dpr;
    canvas.style.width = `${width}px`;
    canvas.style.height = `${height}px`;
    const ctx = canvas.getContext('2d')!;
    ctx.scale(dpr, dpr);
    ctx.lineWidth = 2;
    ctx.lineCap = 'round';
    ctx.strokeStyle = '#111';
  }, [width, height]);

  function pos(e: React.PointerEvent<HTMLCanvasElement>): [number, number] {
    const rect = canvasRef.current!.getBoundingClientRect();
    return [e.clientX - rect.left, e.clientY - rect.top];
  }

  function onDown(e: React.PointerEvent<HTMLCanvasElement>) {
    e.preventDefault();
    canvasRef.current!.setPointerCapture(e.pointerId);
    const ctx = canvasRef.current!.getContext('2d')!;
    const [x, y] = pos(e);
    ctx.beginPath();
    ctx.moveTo(x, y);
    drawingRef.current = true;
  }

  function onMove(e: React.PointerEvent<HTMLCanvasElement>) {
    if (!drawingRef.current) return;
    const ctx = canvasRef.current!.getContext('2d')!;
    const [x, y] = pos(e);
    ctx.lineTo(x, y);
    ctx.stroke();
    if (empty) setEmpty(false);
  }

  function onUp(e: React.PointerEvent<HTMLCanvasElement>) {
    drawingRef.current = false;
    canvasRef.current!.releasePointerCapture(e.pointerId);
  }

  useImperativeHandle(ref, () => ({
    getDataUrl: () => canvasRef.current!.toDataURL('image/png'),
    clear: () => {
      const canvas = canvasRef.current!;
      const ctx = canvas.getContext('2d')!;
      ctx.save();
      ctx.setTransform(1, 0, 0, 1, 0, 0);
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      ctx.restore();
      setEmpty(true);
    },
    isEmpty: () => empty,
  }));

  return (
    <canvas
      ref={canvasRef}
      onPointerDown={onDown}
      onPointerMove={onMove}
      onPointerUp={onUp}
      onPointerCancel={onUp}
      style={{ border: '1px solid #ccc', borderRadius: 4, touchAction: 'none', background: '#fff' }}
    />
  );
});

export default SignaturePad;
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/components/SignaturePad.tsx
git commit -m "feat(frontend): SignaturePad component (raw canvas)"
```

---

## Task 21: Frontend — CustomerForm

**Files:**
- Create: `frontend/src/components/CustomerForm.tsx`

- [ ] **Step 1: Create CustomerForm.tsx**

```tsx
import { useRef, useState } from 'react';
import SignaturePad, { type SignaturePadHandle } from './SignaturePad';
import { createCustomer } from '../api/customerApi';
import type { CustomerDto } from '../types/Customer';

interface Props {
  onSuccess: (customer: CustomerDto) => void;
}

type FieldErrors = Partial<Record<'firstName' | 'lastName' | 'email' | 'phoneNumber' | 'signature' | 'form', string>>;

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const PHONE_RE = /^\+?[0-9]{7,15}$/;

export default function CustomerForm({ onSuccess }: Props) {
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const padRef = useRef<SignaturePadHandle>(null);

  function validate(): FieldErrors {
    const e: FieldErrors = {};
    if (!firstName.trim()) e.firstName = 'Required';
    else if (firstName.length > 100) e.firstName = 'Max 100 chars';
    if (!lastName.trim()) e.lastName = 'Required';
    else if (lastName.length > 100) e.lastName = 'Max 100 chars';
    if (!email.trim()) e.email = 'Required';
    else if (!EMAIL_RE.test(email)) e.email = 'Invalid email';
    if (!phoneNumber.trim()) e.phoneNumber = 'Required';
    else if (!PHONE_RE.test(phoneNumber)) e.phoneNumber = '7-15 digits, optional +';
    if (padRef.current?.isEmpty()) e.signature = 'Signature required';
    return e;
  }

  async function onSubmit(ev: React.FormEvent) {
    ev.preventDefault();
    const e = validate();
    setErrors(e);
    if (Object.keys(e).length) return;

    setSubmitting(true);
    try {
      const dto = await createCustomer({
        firstName,
        lastName,
        email,
        phoneNumber,
        signatureBase64: padRef.current!.getDataUrl(),
      });
      onSuccess(dto);
    } catch (err: unknown) {
      const msg = (err as { detail?: string }).detail ?? 'Submission failed.';
      setErrors({ form: msg });
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form onSubmit={onSubmit} noValidate>
      <h1>Customer Onboarding</h1>

      <label>
        First name
        <input value={firstName} onChange={(e) => setFirstName(e.target.value)} />
        {errors.firstName && <span className="err">{errors.firstName}</span>}
      </label>

      <label>
        Last name
        <input value={lastName} onChange={(e) => setLastName(e.target.value)} />
        {errors.lastName && <span className="err">{errors.lastName}</span>}
      </label>

      <label>
        Email
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
        {errors.email && <span className="err">{errors.email}</span>}
      </label>

      <label>
        Phone number
        <input value={phoneNumber} onChange={(e) => setPhoneNumber(e.target.value)} placeholder="+15551234567" />
        {errors.phoneNumber && <span className="err">{errors.phoneNumber}</span>}
      </label>

      <fieldset>
        <legend>Signature</legend>
        <SignaturePad ref={padRef} />
        <button type="button" onClick={() => padRef.current?.clear()}>Clear</button>
        {errors.signature && <span className="err">{errors.signature}</span>}
      </fieldset>

      {errors.form && <p className="err">{errors.form}</p>}

      <button type="submit" disabled={submitting}>
        {submitting ? 'Submitting…' : 'Register customer'}
      </button>
    </form>
  );
}
```

- [ ] **Step 2: Commit**

```bash
git add frontend/src/components/CustomerForm.tsx
git commit -m "feat(frontend): CustomerForm with validation + signature integration"
```

---

## Task 22: Frontend — ConfirmationCard, App, CSS, main entry

**Files:**
- Create: `frontend/src/components/ConfirmationCard.tsx`
- Create: `frontend/src/App.tsx`
- Create: `frontend/src/App.css`
- Modify: `frontend/src/main.tsx`
- Modify: `frontend/index.html`

- [ ] **Step 1: Create ConfirmationCard.tsx**

```tsx
import type { CustomerDto } from '../types/Customer';

interface Props {
  customer: CustomerDto;
  onAddAnother: () => void;
}

export default function ConfirmationCard({ customer, onAddAnother }: Props) {
  return (
    <div className="confirmation">
      <h1>Registration complete</h1>
      <p>
        <strong>{customer.firstName} {customer.lastName}</strong> has been registered.
      </p>
      <ul>
        <li>Email: {customer.email}</li>
        <li>Phone: {customer.phoneNumber}</li>
        <li>Date: {new Date(customer.dateCreated).toLocaleString()}</li>
      </ul>
      <button type="button" onClick={onAddAnother}>Register another</button>
    </div>
  );
}
```

- [ ] **Step 2: Create App.tsx**

```tsx
import { useState } from 'react';
import CustomerForm from './components/CustomerForm';
import ConfirmationCard from './components/ConfirmationCard';
import type { CustomerDto } from './types/Customer';
import './App.css';

export default function App() {
  const [registered, setRegistered] = useState<CustomerDto | null>(null);

  return (
    <main className="container">
      {registered
        ? <ConfirmationCard customer={registered} onAddAnother={() => setRegistered(null)} />
        : <CustomerForm onSuccess={setRegistered} />}
    </main>
  );
}
```

- [ ] **Step 3: Create App.css**

```css
* { box-sizing: border-box; }

body {
  margin: 0;
  font-family: system-ui, -apple-system, Segoe UI, Roboto, sans-serif;
  background: #f7f7f9;
  color: #111;
}

.container {
  max-width: 540px;
  margin: 40px auto;
  padding: 24px;
  background: #fff;
  border: 1px solid #e1e1e6;
  border-radius: 8px;
  box-shadow: 0 1px 2px rgba(0,0,0,0.04);
}

h1 { margin-top: 0; }

form label {
  display: block;
  margin-bottom: 12px;
  font-size: 0.95rem;
}

form input {
  display: block;
  width: 100%;
  padding: 8px 10px;
  margin-top: 4px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font: inherit;
}

fieldset {
  border: 1px solid #e1e1e6;
  border-radius: 6px;
  padding: 12px;
  margin: 16px 0;
}

button {
  padding: 8px 14px;
  border: 0;
  border-radius: 4px;
  background: #1a73e8;
  color: white;
  font: inherit;
  cursor: pointer;
}
button:disabled { opacity: 0.6; cursor: not-allowed; }
button[type="button"] {
  background: #eee;
  color: #111;
  margin: 8px 0;
}

.err {
  color: #c0392b;
  font-size: 0.85rem;
  margin-left: 8px;
}

.confirmation ul { padding-left: 18px; }
```

- [ ] **Step 4: Set main.tsx**

Overwrite `frontend/src/main.tsx`:
```tsx
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

- [ ] **Step 5: Set index.html title**

Edit `frontend/index.html`: set `<title>` to `Customer Onboarding`, remove any `<link>` to a Vite SVG that no longer exists.

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Customer Onboarding</title>
  </head>
  <body>
    <div id="root"></div>
    <script type="module" src="/src/main.tsx"></script>
  </body>
</html>
```

- [ ] **Step 6: Verify the build**

```bash
cd frontend
npm run build
```
Expected: Build succeeded with no TS errors.

- [ ] **Step 7: Commit**

```bash
cd ..
git add frontend/
git commit -m "feat(frontend): App, ConfirmationCard, styles, main entry"
```

---

## Task 23: End-to-end verification

- [ ] **Step 1: Start the backend (terminal A)**

```bash
dotnet run --project src/Api/CustomerOnboarding.Api.csproj
```
Expected: `Now listening on: http://localhost:5000`.

- [ ] **Step 2: Start the frontend (terminal B)**

```bash
cd frontend
npm run dev
```
Expected: `Local: http://localhost:5173/`.

- [ ] **Step 3: Manual test in browser**

Open http://localhost:5173. Verify:
- All fields visible, signature canvas present.
- Submitting empty shows inline errors.
- Drawing a signature, filling fields, submitting → `ConfirmationCard` appears.
- `customers.db` exists next to the Api binary.
- `GET http://localhost:5000/api/customers` returns the created customer.

- [ ] **Step 4: Stop both servers (Ctrl+C in each).**

- [ ] **Step 5: Run full test suite once more**

```bash
dotnet test CustomerOnboarding.sln
```
Expected: All tests pass.

---

## Task 24: README

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Overwrite README.md**

```markdown
# Customer Onboarding — Reference Solution

Reference build for the 1-hour CS/.NET/React developer technical assessment.

## Stack
- **Backend:** .NET 10 Web API, EF Core 10 + SQLite, MediatR, FluentValidation, AutoMapper, Serilog
- **Frontend:** Vite + React 18 + TypeScript, raw HTML canvas signature pad
- **Tests:** xUnit + Moq + FluentAssertions

## Architecture
Five-project solution with one-way dependencies and a framework-free Domain:

```
src/
  Api/             ASP.NET controllers, middleware, DI composition root
  Application/     MediatR handlers, DTOs, validators, Result<T>
  Domain/          Entity + repository interface (zero NuGet deps)
  Infrastructure/  EF Core DbContext, repository, unit of work
  Tests/           xUnit handler/validator tests
frontend/          Vite + React UI
```

See [docs/superpowers/specs/2026-05-21-customer-onboarding-system-design.md](docs/superpowers/specs/2026-05-21-customer-onboarding-system-design.md) for full design rationale.

## Run

### Backend
```bash
cd src/Api
dotnet run
# API: http://localhost:5000   Swagger: http://localhost:5000/swagger
```
SQLite database (`customers.db`) is created automatically on first run.

### Frontend
```bash
cd frontend
npm install
npm run dev
# UI: http://localhost:5173
```

### Tests
```bash
dotnet test
```

## API

| Method | Path | Body | Success |
|---|---|---|---|
| POST | /api/customers | CreateCustomerRequest | 201 + CustomerDto |
| GET | /api/customers/{id} | — | 200 + CustomerDto |
| GET | /api/customers | — | 200 + CustomerDto[] |
```

- [ ] **Step 2: Commit**

```bash
git add README.md
git commit -m "docs: README with run instructions and architecture overview"
```

---

## Done

All spec requirements covered:

| Requirement | Task |
|---|---|
| Customer create/get/list endpoints | 15 |
| Customer fields (5 + DateCreated) | 4 |
| React onboarding form | 21 |
| Canvas signature pad | 20 |
| Submit + confirmation | 22 |
| Base64 signature storage | 4, 6 |
| Clean layering (Presentation/Application/Infrastructure/API + Domain) | 2 |
| .NET latest stable | 2, 3 |
| No DB calls in controllers | 15 |
| Dependency injection | 11, 13, 16 |
| Input validation | 7 |
| At least one service class (handlers) | 8 |
| SQLite, auto-created, portable | 12, 16 |
| Unit tests for creation + validation | 7, 8 |
| Mock infrastructure | 8 |
| React functional components | 21, 22 |
| Basic frontend validation | 21 |
| Canvas signature | 20 |
| REST calls | 19 |
| Build + run clean | 17, 23 |
| Self-contained, no external services | 12 |
| Run instructions | 24 |
| **Bonus**: logging | 16 |
| **Bonus**: async/await | throughout |
| **Bonus**: error middleware | 14 |
| **Bonus**: DTO mapping | 6 |
