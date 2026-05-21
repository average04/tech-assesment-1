# Coffee Machine API — Design

**Date:** 2026-05-22
**Source spec:** `test/Developer Technical Test..docx` (extracted text in this doc)
**Goal:** A .NET 10 Minimal API exposing a single endpoint `GET /brew-coffee` that satisfies three concrete behaviors plus unit and integration tests. Built as a lean reference solution, separate from the Customer Onboarding work already on `main`.

---

## 1. Source requirements (verbatim from the .docx)

Functional:

1. `GET /brew-coffee` returns 200 OK with a JSON body:
   ```json
   {
     "message": "Your piping hot coffee is ready",
     "prepared": "2021-02-03T11:56:24+0900"
   }
   ```
   Date/time formatted as ISO-8601.
2. Every fifth call to the endpoint returns 503 Service Unavailable with an empty body (signifying the coffee machine is out of coffee).
3. On April 1st, all calls return 418 I'm a teapot with an empty body.

Non-functional:

- Implemented in .NET Core (we'll target .NET 10).
- Include unit and/or integration tests.

Extra credit (NOT in scope for this branch — see Section 9): weather service integration that changes the message above 30°C. Will live on `feat/coffee-weather`.

---

## 2. Behavior precedence (the one spec ambiguity)

> "If the date is April 1st, then **all** calls to the endpoint defined in #1 should return 418 I'm a teapot instead"

The word "all" is taken at face value: on April 1st, requirement #3 overrides #2. Every call returns 418 — including would-be 5th calls. The 5th-call counter still increments (so the day after April 1st, the counter is consistent with the number of calls made), but the 503 logic is suppressed for the day.

The alternative reading (503 still wins on the 5th call) is rejected because it directly contradicts "all calls."

---

## 3. Solution Layout

```
coffee-machine/
  CoffeeMachine.sln
  src/
    Api/
      CoffeeMachine.Api.csproj
      Program.cs                          minimal API; DI; route mapping
      Services/IBrewCounter.cs
      Services/BrewCounter.cs             Interlocked.Increment singleton
      Services/IBrewService.cs
      Services/BrewService.cs             pure logic; takes TimeProvider
      Models/BrewResponse.cs              record (Message, Prepared)
      Models/BrewOutcome.cs               discriminated outcome the endpoint maps to a status code
    Tests/
      CoffeeMachine.Tests.csproj
      BrewServiceTests.cs                 unit tests against BrewService
      BrewEndpointTests.cs                integration tests via WebApplicationFactory
  README.md
```

The `coffee-machine/` directory is a self-contained .NET solution. It does NOT share projects, packages, or build outputs with the Customer Onboarding work in the repo's top-level `src/`.

---

## 4. Architecture

### Minimal API style

`Program.cs` uses top-level statements + `WebApplication.CreateBuilder` + `app.MapGet("/brew-coffee", handler)`. No controllers, no MediatR, no AutoMapper — the spec is one endpoint with three branches; controllers would be ceremony.

### Components

| Component | Lifetime | Responsibility |
|---|---|---|
| `IBrewCounter` / `BrewCounter` | Singleton | Holds the in-process call count. `Next()` returns the new count via `Interlocked.Increment`. |
| `TimeProvider` | Singleton (`TimeProvider.System`) | Built-in .NET 8+ clock abstraction. Real impl in prod; `FakeTimeProvider` (from `Microsoft.Extensions.TimeProvider.Testing`) in tests. |
| `IBrewService` / `BrewService` | Singleton | Pure decision function: `Brew()` → returns one of three outcomes. Consumes `TimeProvider` + `IBrewCounter`. |
| `BrewOutcome` | n/a | Discriminated union (sealed class hierarchy or `OneOf`-style record). The endpoint maps it to a status code. |
| `BrewResponse` | n/a | DTO returned in the 200 body. |

### Decision logic (in `BrewService.Brew()`)

```
n = counter.Next()
now = timeProvider.GetLocalNow() // or GetUtcNow() — see Section 5
if now.Month == 4 && now.Day == 1: return Teapot
if n % 5 == 0:                     return OutOfCoffee
return Ready(message, prepared = now)
```

### Endpoint handler (in `Program.cs`)

The handler calls `IBrewService.Brew()` and switches on the result:

| `BrewOutcome` | HTTP response |
|---|---|
| `Ready(response)` | `Results.Ok(response)` |
| `OutOfCoffee` | `Results.StatusCode(503)` (no body) |
| `Teapot` | `Results.StatusCode(418)` (no body) |

---

## 5. Time zone considerations

The April 1st check depends on which clock we use. Options:
- **UTC** — predictable, but a request made on April 1st in Sydney at 10am AEDT would see UTC March 31st and not get a teapot.
- **Server local time** — matches the operator's intuition ("today is April 1st here") but couples behavior to deployment timezone.
- **Configured timezone** — most flexible, most ceremony.

**Decision:** Use `TimeProvider.GetLocalNow()` (server local time). It matches the spec's casual phrasing ("the date is April 1st") and avoids adding a config knob for a joke holiday. Tests inject a `FakeTimeProvider` with whatever local time they need.

The `prepared` field in the 200 response also uses local time, formatted via `ToString("o")` (round-trip ISO-8601) which includes the offset — e.g. `2026-05-22T14:30:00.0000000+08:00`.

---

## 6. Testing

### Unit tests — `BrewServiceTests.cs`

Tests against `BrewService` directly with `FakeTimeProvider` + a stub `IBrewCounter`. Covers:

- Call #1 on a non-April-1st date → `Ready` with the expected message
- Call #4 → `Ready`; Call #5 → `OutOfCoffee`; Call #6 → `Ready`
- Call #10 → `OutOfCoffee`
- Any call on April 1st (any year) → `Teapot`, even on the 5th call
- The `Prepared` value matches the injected `TimeProvider`'s now
- Counter advances every call regardless of outcome (so 5th-call cadence stays consistent after April 1st)

### Integration tests — `BrewEndpointTests.cs`

Uses `WebApplicationFactory<Program>` to spin up the in-memory test server. To inject the `FakeTimeProvider`, the factory replaces the registered `TimeProvider`. Tests:

- `GET /brew-coffee` returns 200 + body matching the schema (`message`, `prepared` parseable as ISO-8601)
- 5 sequential calls: calls 1–4 return 200, call 5 returns 503 with empty body
- With `FakeTimeProvider` set to April 1st: returns 418 + empty body
- April 1st + 5th call: still 418 (teapot wins per Section 2)

### Test packages

- `xunit` + `xunit.runner.visualstudio`
- `Microsoft.AspNetCore.Mvc.Testing` (for `WebApplicationFactory<TEntryPoint>`)
- `Microsoft.Extensions.TimeProvider.Testing` (for `FakeTimeProvider`)
- `FluentAssertions`

---

## 7. Run instructions (target README)

```bash
cd coffee-machine
dotnet run --project src/Api
# Listening on http://localhost:5050

curl http://localhost:5050/brew-coffee
```

Tests:
```bash
cd coffee-machine
dotnet test
```

Port `5050` chosen to avoid colliding with the Customer Onboarding API on `5000` and the Vite dev server on `5173`.

---

## 8. Out of scope (intentional YAGNI)

- Authentication, rate limiting, observability beyond the default console logger.
- Persistence — the counter is in-memory; restarting the process resets it. See brainstorming decision: "in-memory only" was selected.
- HTTPS — the spec doesn't require it; `dotnet run` defaults to HTTP for local dev.
- Configurable schedule (e.g., variable "every Nth call" or different teapot day). The numbers in the spec are constants.
- Mapping the 503/418 bodies to ProblemDetails. The spec explicitly says **empty response body**.

---

## 9. Extra credit — separate branch

The .docx's "Extra Credit" section adds a weather service integration that changes the message above 30°C. Per the .docx submission instructions ("supply the solution separately to the original – for instance, as a second Zip file, or as a Git branch"), this work will land on a separate branch `feat/coffee-weather` derived from main *after* this spec ships.

It's explicitly out of scope for this spec and plan. A separate spec + plan will be created when we start the extra credit.

---

## 10. Branch and integration plan

- `main` already contains the Customer Onboarding solution (different folder).
- This coffee API will be committed directly to `main` alongside it. The two solutions are independent: each has its own `.sln`, its own `dotnet test`, its own README.
- Repo root README will be updated to mention both projects with one-liners pointing at each.
- The stale `feat/bonus-extras` branch (created earlier when the .docx was misread) will be deleted before work begins.
