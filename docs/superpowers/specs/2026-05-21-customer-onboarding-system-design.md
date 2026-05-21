# Customer Registration & Onboarding System — Design

**Date:** 2026-05-21
**Source:** [test/Development-Assessment-CS-DotNet-React-2026-02.md](../../../test/Development-Assessment-CS-DotNet-React-2026-02.md)
**Intent:** Reference solution for the 1-hour developer technical test. Optimized for architectural clarity over feature breadth.

---

## 1. Goal

Build a full-stack Customer Registration & Onboarding system:

- **Backend:** .NET 10 Web API exposing create / get-by-id / list endpoints
- **Frontend:** React form with HTML canvas signature pad
- **Persistence:** SQLite, auto-created on first run

The assessment evaluates architecture, separation of concerns, testability, and end-to-end function — *not* feature completeness. The design favors a textbook clean-architecture layout to make the boundaries visible at a glance.

---

## 2. Solution Layout

```
/src
  /Api               .NET 10 Web API — controllers, middleware, DI composition root
  /Application       MediatR handlers, DTOs, AutoMapper profiles, FluentValidation, Result<T>
  /Domain            Customer entity, repository interfaces (zero NuGet deps)
  /Infrastructure    EF Core DbContext, SQLite, CustomerRepository, UnitOfWork
  /Tests             xUnit + Moq + FluentAssertions
/frontend            Vite + React + TypeScript
README.md
```

### Project reference graph (one-way only)

| Project | References |
|---|---|
| `Api` | `Application`, `Infrastructure` |
| `Application` | `Domain` |
| `Infrastructure` | `Application`, `Domain` |
| `Tests` | `Application`, `Domain`, `Infrastructure` |
| `Domain` | *(nothing)* |

The Domain project has no NuGet dependencies. This is the rule that makes the architecture demonstrably clean.

---

## 3. Domain

### `Customer` entity

| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key, generated in domain |
| `FirstName` | `string` | Required, ≤100 chars |
| `LastName` | `string` | Required, ≤100 chars |
| `Email` | `string` | Required, valid format, unique |
| `PhoneNumber` | `string` | Required, E.164-ish format |
| `SignatureBase64` | `string` | Base64-encoded PNG data URL |
| `DateCreated` | `DateTime` | UTC, set at creation |

### Domain interfaces

- `ICustomerRepository` — `AddAsync`, `GetByIdAsync`, `ListAsync`, `ExistsByEmailAsync`
- `IUnitOfWork` — `SaveChangesAsync`

Both live in Domain. Implementations live in Infrastructure.

---

## 4. Application Layer

### MediatR requests + handlers

| Request | Handler | Returns |
|---|---|---|
| `CreateCustomerCommand` | `CreateCustomerHandler` | `Result<CustomerDto>` |
| `GetCustomerByIdQuery` | `GetCustomerByIdHandler` | `Result<CustomerDto>` |
| `ListCustomersQuery` | `ListCustomersHandler` | `Result<IReadOnlyList<CustomerDto>>` |

### DTOs

- `CustomerDto` — what the API returns (no `SignatureBase64` in list view to keep payloads small; included in by-id view)
- `CreateCustomerRequest` — what the API accepts (includes signature)

### Validation (`FluentValidation`)

`CreateCustomerValidator`:
- First/Last name: required, ≤100 chars
- Email: required, valid format
- Phone: required, regex match
- Signature: required, must be valid base64 PNG data URL prefix (`data:image/png;base64,`)

Uniqueness is enforced in the **handler**, not the validator, because it requires a repo call.

### Pipeline behavior

`ValidationBehavior<TRequest, TResponse>` runs FluentValidation before each handler. On failure it short-circuits with `Result.Fail` carrying field-level errors.

### Result pattern

Custom `Result` / `Result<T>` with `IsSuccess`, `Value`, `Errors[]`, `ErrorType` (Validation | NotFound | Conflict | Unexpected). No exceptions for control flow.

### AutoMapper

`MappingProfile` defines `CreateCustomerRequest → Customer` and `Customer → CustomerDto`.

---

## 5. Infrastructure

- `AppDbContext : DbContext` — single `DbSet<Customer>`, fluent config for max lengths and unique email index
- `CustomerRepository : ICustomerRepository` — wraps `AppDbContext`
- `UnitOfWork : IUnitOfWork` — wraps `AppDbContext.SaveChangesAsync`
- Connection string: `Data Source=customers.db` (relative to Api working dir)
- Bootstrap: `Database.EnsureCreated()` at startup — no migrations to keep first-run friction zero

---

## 6. API Layer

### Endpoints

| Method | Path | Body | Success | Failure |
|---|---|---|---|---|
| POST | `/api/customers` | `CreateCustomerRequest` | 201 + `CustomerDto` + `Location` | 400 validation, 409 duplicate email |
| GET | `/api/customers/{id:guid}` | — | 200 + `CustomerDto` | 404 |
| GET | `/api/customers` | — | 200 + `CustomerDto[]` | — |

### Controller pattern

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateCustomerRequest req, CancellationToken ct)
{
    var result = await _mediator.Send(new CreateCustomerCommand(req), ct);
    return result.ToActionResult(this); // extension that maps Result → IActionResult
}
```

Controllers are thin — they translate HTTP ↔ MediatR and `Result` ↔ status codes. No business logic, no EF Core, no `try/catch`.

### Middleware

- `ExceptionHandlingMiddleware` — catches unexpected exceptions, logs them, returns RFC 7807 `ProblemDetails` with 500.
- Serilog request logging — one structured log line per request.

### Cross-cutting

- **CORS** — open to `http://localhost:5173` in Development
- **Logging** — Serilog → Console + `logs/log-.txt` daily rolling
- **Async** — every handler/repo/controller method is async with `CancellationToken`
- **Swagger** — enabled in Development for manual API exploration

---

## 7. Frontend

### Stack

Vite + React 18 + TypeScript. Functional components, hooks only. No styling framework — one `App.css`.

### Structure

```
/frontend
  /src
    /api
      customerApi.ts        typed fetch wrapper
    /components
      CustomerForm.tsx      controlled inputs + field errors
      SignaturePad.tsx      raw <canvas> + pointer events
      ConfirmationCard.tsx  post-submit success view
    /types
      Customer.ts           DTO shape mirroring backend
    App.tsx                 form ↔ confirmation state
    main.tsx
    App.css
  vite.config.ts            proxy /api → http://localhost:5000
  .env                      VITE_API_BASE_URL
```

### `SignaturePad`

Implemented from scratch with `<canvas>` + `pointerdown / pointermove / pointerup`. The spec specifically requires "HTML canvas," so no `react-signature-canvas` wrapper — the canvas work is visible. Exposes `getDataUrl()` and `clear()` via `useImperativeHandle`.

Approx. 40 lines, including hi-DPI scaling so the line stays crisp.

### `CustomerForm`

- Controlled inputs for First Name, Last Name, Email, Phone
- Embedded `SignaturePad` with a Clear button
- Submit disabled until all fields filled + signature non-empty
- Inline field validation on blur (required, basic email + phone regex)
- On 201 → swap to `ConfirmationCard`
- On 4xx → render field errors from the ProblemDetails response

### `ConfirmationCard`

Shows submitted name + email + a "Register Another" button that resets the form.

---

## 8. Testing

### Framework

xUnit + Moq + FluentAssertions. Tests live in `/src/Tests`.

### Coverage

| Test class | Cases |
|---|---|
| `CreateCustomerHandlerTests` | Happy path (maps, calls Add + Save once, returns Ok); duplicate email returns Fail + never saves; repo throws surfaces failure |
| `CreateCustomerValidatorTests` | One pass + one fail per rule (required, length, email format, phone format, base64 signature) |
| `GetCustomerByIdHandlerTests` | Found returns DTO; not-found returns Result.Fail(NotFound) |
| `ListCustomersHandlerTests` | Empty → empty list; multiple → ordered by DateCreated desc |

`ICustomerRepository` and `IUnitOfWork` are mocked. No EF Core or SQLite in tests — the Domain's framework-free design makes this clean.

The spec asks for "creation logic + validation rules" only. The other two are cheap freebies given the handler pattern.

---

## 9. Run Instructions (target README)

```bash
# Backend
cd src/Api
dotnet run            # API at http://localhost:5000, Swagger at /swagger

# Frontend
cd frontend
npm install
npm run dev           # UI at http://localhost:5173
```

SQLite database (`customers.db`) is created automatically on first run alongside the Api binary.

---

## 10. Out of Scope

- Authentication / authorization
- Pagination / search / filtering
- File storage for signatures (kept inline as base64 per spec — file storage is offered as an alternative, but inline is simpler and self-contained)
- Migrations (`EnsureCreated` is enough for the demo)
- Production hardening (HTTPS redirect, rate limiting, secrets management)
- Internationalization

---

## 11. Bonus items included

All four optional bonuses from the spec:

- Logging — Serilog
- Async/await — throughout
- Error-handling middleware — `ExceptionHandlingMiddleware`
- DTO mapping — AutoMapper
