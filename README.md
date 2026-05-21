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
