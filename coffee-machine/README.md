# Coffee Machine API

Minimal API satisfying the .docx assessment in `../test/Developer Technical Test..docx`.

## Behaviors
- `GET /brew-coffee` → 200 OK with `{ "message": "Your piping hot coffee is ready", "prepared": "<ISO-8601>" }`
- Every 5th call → 503 Service Unavailable (empty body)
- On April 1st → 418 I'm a teapot (empty body) — overrides the 5th-call rule

## Stack
.NET 10 Minimal API, `TimeProvider` for testable clock, `WebApplicationFactory` for integration tests, FluentAssertions 6.x.

## Run

```bash
cd coffee-machine
dotnet run --project src/Api
# API: http://localhost:5050

curl http://localhost:5050/brew-coffee
```

## Test

```bash
cd coffee-machine
dotnet test
```

Expected: 18 tests pass (13 unit + 5 integration).

## Design notes
See [../docs/superpowers/specs/2026-05-22-coffee-machine-api-design.md](../docs/superpowers/specs/2026-05-22-coffee-machine-api-design.md).

The "Extra Credit" weather-aware variant from the .docx will live on branch `feat/coffee-weather`.
