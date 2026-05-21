# Coffee Machine API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a .NET 10 Minimal API exposing `GET /brew-coffee` with three precedence-ordered behaviors (200 OK, 503 every 5th call, 418 on April 1st), plus unit and integration tests. Self-contained in `coffee-machine/` alongside the existing Customer Onboarding work.

**Architecture:** Single Minimal API project + a test project. Endpoint handler delegates to a pure `IBrewService` whose decision logic depends on injected `TimeProvider` and `IBrewCounter` (singleton, `Interlocked.Increment`). Outcomes modeled as a sealed-record discriminated union, which the endpoint pattern-matches to status codes.

**Tech Stack:** .NET 10, ASP.NET Core Minimal API, xUnit, `WebApplicationFactory<Program>`, `Microsoft.Extensions.TimeProvider.Testing` (FakeTimeProvider), FluentAssertions 6.x (last MIT).

**Spec:** [docs/superpowers/specs/2026-05-22-coffee-machine-api-design.md](../specs/2026-05-22-coffee-machine-api-design.md)

---

## File Structure (locked in)

```
coffee-machine/
  CoffeeMachine.sln
  README.md
  src/
    Api/
      CoffeeMachine.Api.csproj
      Program.cs
      Models/BrewResponse.cs
      Models/BrewOutcome.cs
      Services/IBrewCounter.cs
      Services/BrewCounter.cs
      Services/IBrewService.cs
      Services/BrewService.cs
      appsettings.json
      appsettings.Development.json
      Properties/launchSettings.json
    Tests/
      CoffeeMachine.Tests.csproj
      BrewServiceTests.cs
      BrewEndpointTests.cs
```

Plus a one-line update to the repo root `README.md`.

---

## Task 1: Delete stale `feat/bonus-extras` branch

The branch was created earlier under a misreading of the spec. It's a pointer at the same commit as `main` and has no unique commits. Safe to delete.

**Files:** None (git operation only).

- [ ] **Step 1: Confirm branch has no unique commits**

```bash
cd d:/Projects/tech-assement-1
git log feat/bonus-extras --not main --oneline
```
Expected: empty output (no commits unique to `feat/bonus-extras`).

- [ ] **Step 2: Delete the branch**

```bash
git branch -d feat/bonus-extras
```
Expected: `Deleted branch feat/bonus-extras (was dcbcdb3).`

- [ ] **Step 3: Confirm only `main` remains locally**

```bash
git branch
```
Expected: only `* main`.

---

## Task 2: Scaffold `coffee-machine/` solution + 2 projects + references

**Files:**
- Create: `coffee-machine/CoffeeMachine.sln`
- Create: `coffee-machine/src/Api/CoffeeMachine.Api.csproj` (via `dotnet new web`)
- Create: `coffee-machine/src/Tests/CoffeeMachine.Tests.csproj` (via `dotnet new xunit`)

- [ ] **Step 1: Create directory tree + solution**

```bash
cd d:/Projects/tech-assement-1
mkdir -p coffee-machine/src
cd coffee-machine
dotnet new sln -n CoffeeMachine
```
Expected: `CoffeeMachine.sln` created under `coffee-machine/`.

- [ ] **Step 2: Create the Api project (Minimal API template)**

```bash
dotnet new web -n CoffeeMachine.Api -o src/Api --no-https
```

Note: `dotnet new web` produces a Minimal API template (single `Program.cs` with `app.MapGet("/", ...)`), which is exactly the style we want. The `--no-https` skips the default HTTPS profile to simplify local dev (the spec doesn't require HTTPS).

- [ ] **Step 3: Create the Tests project**

```bash
dotnet new xunit -n CoffeeMachine.Tests -o src/Tests
```

- [ ] **Step 4: Add projects to the solution**

```bash
dotnet sln add src/Api/CoffeeMachine.Api.csproj
dotnet sln add src/Tests/CoffeeMachine.Tests.csproj
```

- [ ] **Step 5: Wire reference Tests → Api**

```bash
dotnet add src/Tests/CoffeeMachine.Tests.csproj reference src/Api/CoffeeMachine.Api.csproj
```

- [ ] **Step 6: Delete the default `UnitTest1.cs` and the Api template's `Program.cs` body (we'll rewrite Program.cs in Task 7)**

```bash
rm -f src/Tests/UnitTest1.cs
```
Leave `src/Api/Program.cs` alone for now — it'll be overwritten in Task 7.

- [ ] **Step 7: Verify build**

```bash
dotnet build CoffeeMachine.sln
```
Expected: `Build succeeded.` with 0 errors.

- [ ] **Step 8: Commit**

```bash
cd ..
git add coffee-machine/
git commit -m "chore(coffee): scaffold solution with Api + Tests projects"
```

---

## Task 3: Install NuGet packages

**Files:** `coffee-machine/src/Api/CoffeeMachine.Api.csproj` and `coffee-machine/src/Tests/CoffeeMachine.Tests.csproj` (modified by `dotnet add package`).

- [ ] **Step 1: Tests packages**

Pin FluentAssertions to 6.12.2 (last MIT version; v8+ is commercial).

```bash
cd d:/Projects/tech-assement-1/coffee-machine
dotnet add src/Tests/CoffeeMachine.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add src/Tests/CoffeeMachine.Tests.csproj package Microsoft.Extensions.TimeProvider.Testing
dotnet add src/Tests/CoffeeMachine.Tests.csproj package FluentAssertions --version 6.12.2
```

- [ ] **Step 2: Verify build**

```bash
dotnet build CoffeeMachine.sln
```
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
cd ..
git add coffee-machine/src/
git commit -m "chore(coffee): add test packages (Mvc.Testing, FakeTimeProvider, FluentAssertions)"
```

---

## Task 4: Models — `BrewResponse` and `BrewOutcome`

**Files:**
- Create: `coffee-machine/src/Api/Models/BrewResponse.cs`
- Create: `coffee-machine/src/Api/Models/BrewOutcome.cs`

- [ ] **Step 1: Create BrewResponse.cs**

```csharp
namespace CoffeeMachine.Api.Models;

public record BrewResponse(string Message, DateTimeOffset Prepared);
```

- [ ] **Step 2: Create BrewOutcome.cs**

```csharp
namespace CoffeeMachine.Api.Models;

public abstract record BrewOutcome
{
    public sealed record Ready(BrewResponse Response) : BrewOutcome;
    public sealed record OutOfCoffee : BrewOutcome
    {
        public static readonly OutOfCoffee Instance = new();
    }
    public sealed record Teapot : BrewOutcome
    {
        public static readonly Teapot Instance = new();
    }
}
```

The `Instance` singletons let consumers avoid allocating a new `OutOfCoffee` / `Teapot` on every decision.

- [ ] **Step 3: Verify build**

```bash
cd d:/Projects/tech-assement-1/coffee-machine
dotnet build src/Api/CoffeeMachine.Api.csproj
```
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
cd ..
git add coffee-machine/src/Api/Models/
git commit -m "feat(coffee): BrewResponse and BrewOutcome (sealed-record DU)"
```

---

## Task 5: BrewCounter (singleton, thread-safe)

**Files:**
- Create: `coffee-machine/src/Api/Services/IBrewCounter.cs`
- Create: `coffee-machine/src/Api/Services/BrewCounter.cs`

- [ ] **Step 1: Create IBrewCounter.cs**

```csharp
namespace CoffeeMachine.Api.Services;

public interface IBrewCounter
{
    int Next();
}
```

`Next()` returns the **new** count after the increment (so the very first call returns 1).

- [ ] **Step 2: Create BrewCounter.cs**

```csharp
namespace CoffeeMachine.Api.Services;

public sealed class BrewCounter : IBrewCounter
{
    private int _count;

    public int Next() => Interlocked.Increment(ref _count);
}
```

- [ ] **Step 3: Verify build**

```bash
cd d:/Projects/tech-assement-1/coffee-machine
dotnet build src/Api/CoffeeMachine.Api.csproj
```
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
cd ..
git add coffee-machine/src/Api/Services/IBrewCounter.cs coffee-machine/src/Api/Services/BrewCounter.cs
git commit -m "feat(coffee): IBrewCounter + thread-safe BrewCounter"
```

---

## Task 6: BrewService (TDD)

**Files:**
- Create: `coffee-machine/src/Api/Services/IBrewService.cs`
- Create: `coffee-machine/src/Api/Services/BrewService.cs`
- Create: `coffee-machine/src/Tests/BrewServiceTests.cs`

- [ ] **Step 1: Create IBrewService.cs**

```csharp
using CoffeeMachine.Api.Models;

namespace CoffeeMachine.Api.Services;

public interface IBrewService
{
    BrewOutcome Brew();
}
```

- [ ] **Step 2: Write the failing tests**

Create `coffee-machine/src/Tests/BrewServiceTests.cs`:

```csharp
using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CoffeeMachine.Tests;

public class BrewServiceTests
{
    private const string HotMessage = "Your piping hot coffee is ready";

    private static FakeTimeProvider FakeTime(DateTimeOffset now)
    {
        var tp = new FakeTimeProvider(now);
        return tp;
    }

    private sealed class CountingCounter : IBrewCounter
    {
        private int _n;
        public int Next() => ++_n;
    }

    [Fact]
    public void First_call_returns_ready_with_hot_message_and_now()
    {
        var now = new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero);
        var sut = new BrewService(FakeTime(now), new CountingCounter());

        var outcome = sut.Brew();

        outcome.Should().BeOfType<BrewOutcome.Ready>()
            .Which.Response.Should().Be(new BrewResponse(HotMessage, now));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(9)]
    public void Non_fifth_calls_return_ready(int callNumber)
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());
        BrewOutcome? last = null;
        for (var i = 0; i < callNumber; i++) last = sut.Brew();
        last.Should().BeOfType<BrewOutcome.Ready>();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public void Every_fifth_call_returns_out_of_coffee(int callNumber)
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());
        BrewOutcome? last = null;
        for (var i = 0; i < callNumber; i++) last = sut.Brew();
        last.Should().BeOfType<BrewOutcome.OutOfCoffee>();
    }

    [Fact]
    public void April_first_returns_teapot_on_first_call()
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());

        var outcome = sut.Brew();

        outcome.Should().BeOfType<BrewOutcome.Teapot>();
    }

    [Fact]
    public void April_first_returns_teapot_even_on_fifth_call()
    {
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero)),
                                  new CountingCounter());
        BrewOutcome? last = null;
        for (var i = 0; i < 5; i++) last = sut.Brew();
        last.Should().BeOfType<BrewOutcome.Teapot>();
    }

    [Fact]
    public void Counter_advances_on_every_call_even_on_april_first()
    {
        var counter = new CountingCounter();
        var sut = new BrewService(FakeTime(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero)), counter);
        for (var i = 0; i < 3; i++) sut.Brew();
        counter.Next().Should().Be(4); // 3 calls done, next is the 4th
    }
}
```

- [ ] **Step 3: Run tests to confirm RED**

```bash
cd d:/Projects/tech-assement-1/coffee-machine
dotnet test src/Tests/CoffeeMachine.Tests.csproj
```
Expected: Build error — `BrewService` does not exist yet.

- [ ] **Step 4: Implement BrewService.cs**

Create `coffee-machine/src/Api/Services/BrewService.cs`:

```csharp
using CoffeeMachine.Api.Models;

namespace CoffeeMachine.Api.Services;

public sealed class BrewService : IBrewService
{
    private const string HotMessage = "Your piping hot coffee is ready";

    private readonly TimeProvider _time;
    private readonly IBrewCounter _counter;

    public BrewService(TimeProvider time, IBrewCounter counter)
    {
        _time = time;
        _counter = counter;
    }

    public BrewOutcome Brew()
    {
        var callNumber = _counter.Next();
        var now = _time.GetLocalNow();

        if (now.Month == 4 && now.Day == 1)
        {
            return BrewOutcome.Teapot.Instance;
        }

        if (callNumber % 5 == 0)
        {
            return BrewOutcome.OutOfCoffee.Instance;
        }

        return new BrewOutcome.Ready(new BrewResponse(HotMessage, now));
    }
}
```

- [ ] **Step 5: Run tests to confirm GREEN**

```bash
dotnet test src/Tests/CoffeeMachine.Tests.csproj
```
Expected: All tests pass (13 unit tests).

- [ ] **Step 6: Commit**

```bash
cd ..
git add coffee-machine/src/Api/Services/IBrewService.cs coffee-machine/src/Api/Services/BrewService.cs coffee-machine/src/Tests/BrewServiceTests.cs
git commit -m "feat(coffee): BrewService with TDD (200/503/418 precedence)"
```

---

## Task 7: Wire `Program.cs` (Minimal API)

**Files:**
- Modify: `coffee-machine/src/Api/Program.cs` (full rewrite from the template default)
- Modify: `coffee-machine/src/Api/Properties/launchSettings.json` (set port to 5050)

- [ ] **Step 1: Overwrite Program.cs**

```csharp
using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IBrewCounter, BrewCounter>();
builder.Services.AddSingleton<IBrewService, BrewService>();

var app = builder.Build();

app.MapGet("/brew-coffee", (IBrewService brewService) =>
{
    var outcome = brewService.Brew();
    return outcome switch
    {
        BrewOutcome.Ready ready => Results.Ok(ready.Response),
        BrewOutcome.OutOfCoffee => Results.StatusCode(StatusCodes.Status503ServiceUnavailable),
        BrewOutcome.Teapot => Results.StatusCode(StatusCodes.Status418ImATeapot),
        _ => Results.StatusCode(StatusCodes.Status500InternalServerError)
    };
});

app.Run();

public partial class Program { }
```

The `public partial class Program { }` at the bottom is required so `WebApplicationFactory<Program>` in the test project can reference the entry point. Top-level statements generate a non-public `Program` class by default; this declaration promotes it to public.

- [ ] **Step 2: Set launchSettings.json**

Overwrite `coffee-machine/src/Api/Properties/launchSettings.json`:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5050",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

- [ ] **Step 3: Build the whole solution**

```bash
cd d:/Projects/tech-assement-1/coffee-machine
dotnet build CoffeeMachine.sln
```
Expected: Build succeeded.

- [ ] **Step 4: Re-run unit tests (no regression)**

```bash
dotnet test src/Tests/CoffeeMachine.Tests.csproj
```
Expected: All 13 tests still pass.

- [ ] **Step 5: Commit**

```bash
cd ..
git add coffee-machine/src/Api/Program.cs coffee-machine/src/Api/Properties/launchSettings.json
git commit -m "feat(coffee): minimal API endpoint with status-code mapping"
```

---

## Task 8: Integration tests with `WebApplicationFactory`

**Files:**
- Create: `coffee-machine/src/Tests/BrewEndpointTests.cs`

- [ ] **Step 1: Create BrewEndpointTests.cs**

```csharp
using System.Net;
using System.Net.Http.Json;
using CoffeeMachine.Api.Models;
using CoffeeMachine.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace CoffeeMachine.Tests;

public class BrewEndpointTests
{
    private sealed class TestFactory : WebApplicationFactory<Program>
    {
        public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero));

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(Time);
            });
        }
    }

    [Fact]
    public async Task First_call_returns_200_with_expected_shape()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<BrewResponse>();
        body.Should().NotBeNull();
        body!.Message.Should().Be("Your piping hot coffee is ready");
        body.Prepared.Should().Be(new DateTimeOffset(2026, 5, 22, 14, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Fifth_call_returns_503_with_empty_body()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient();

        HttpResponseMessage? last = null;
        for (var i = 0; i < 5; i++) last = await client.GetAsync("/brew-coffee");

        last!.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        (await last.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Sixth_call_returns_200_again()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient();

        for (var i = 0; i < 5; i++) await client.GetAsync("/brew-coffee");
        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task April_first_returns_418_with_empty_body()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero));
        var client = factory.CreateClient();

        var res = await client.GetAsync("/brew-coffee");

        res.StatusCode.Should().Be(HttpStatusCode.ImATeapot);
        (await res.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task April_first_returns_418_even_on_fifth_call()
    {
        await using var factory = new TestFactory();
        factory.Time.SetUtcNow(new DateTimeOffset(2026, 4, 1, 9, 0, 0, TimeSpan.Zero));
        var client = factory.CreateClient();

        HttpResponseMessage? last = null;
        for (var i = 0; i < 5; i++) last = await client.GetAsync("/brew-coffee");

        last!.StatusCode.Should().Be(HttpStatusCode.ImATeapot);
    }
}
```

- [ ] **Step 2: Run integration tests**

```bash
cd d:/Projects/tech-assement-1/coffee-machine
dotnet test src/Tests/CoffeeMachine.Tests.csproj
```
Expected: All tests pass — 13 unit + 5 integration = 18 total.

- [ ] **Step 3: Commit**

```bash
cd ..
git add coffee-machine/src/Tests/BrewEndpointTests.cs
git commit -m "test(coffee): integration tests via WebApplicationFactory + FakeTimeProvider"
```

---

## Task 9: Manual smoke verification (no commit — verification only)

- [ ] **Step 1: Start the API**

```bash
cd d:/Projects/tech-assement-1/coffee-machine
dotnet run --project src/Api/CoffeeMachine.Api.csproj
```
Expected: `Now listening on: http://localhost:5050`.

- [ ] **Step 2: In a separate terminal, hit the endpoint 6 times**

```bash
for i in 1 2 3 4 5 6; do
  echo "--- Call $i ---"
  curl -sS -i http://localhost:5050/brew-coffee | head -1
done
```
Expected: calls 1, 2, 3, 4, 6 → `HTTP/1.1 200 OK`; call 5 → `HTTP/1.1 503 Service Unavailable`.

- [ ] **Step 3: Inspect a 200 body for shape**

```bash
curl -sS http://localhost:5050/brew-coffee | tee /tmp/brew.json
```
Expected: a JSON object with `message` = `"Your piping hot coffee is ready"` and `prepared` matching an ISO-8601 string.

- [ ] **Step 4: Stop the API (Ctrl+C).**

If anything in steps 1–3 doesn't match, treat it as a defect: stop, fix, re-run unit + integration tests, then return here.

---

## Task 10: `coffee-machine/README.md`

**Files:**
- Create: `coffee-machine/README.md`

- [ ] **Step 1: Create the README**

```markdown
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
```

- [ ] **Step 2: Commit**

```bash
cd d:/Projects/tech-assement-1
git add coffee-machine/README.md
git commit -m "docs(coffee): README with run/test instructions and links to spec"
```

---

## Task 11: Update repo root README to mention both projects

**Files:**
- Modify: `d:/Projects/tech-assement-1/README.md`

- [ ] **Step 1: Read the current root README to understand current content**

```bash
cat d:/Projects/tech-assement-1/README.md
```

It currently describes only the Customer Onboarding solution.

- [ ] **Step 2: Overwrite README.md to cover both projects**

```markdown
# Technical Assessment — Reference Solutions

This repository contains reference solutions for two separate developer technical assessments, both in `./test/`.

## Projects

### Customer Onboarding (`src/` + `frontend/`)
.NET 10 + EF Core SQLite backend with a Vite/React frontend featuring a canvas signature pad. Satisfies `test/Development-Assessment-CS-DotNet-React-2026-02.md`.

- Run backend: `cd src/Api && dotnet run` (http://localhost:5000)
- Run frontend: `cd frontend && npm install && npm run dev` (http://localhost:5173)
- Test: `dotnet test`
- Design: [docs/superpowers/specs/2026-05-21-customer-onboarding-system-design.md](docs/superpowers/specs/2026-05-21-customer-onboarding-system-design.md)

### Coffee Machine API (`coffee-machine/`)
.NET 10 Minimal API satisfying `test/Developer Technical Test..docx`. Single endpoint `GET /brew-coffee` with three behaviors (200/503/418).

- Run: `cd coffee-machine && dotnet run --project src/Api` (http://localhost:5050)
- Test: `cd coffee-machine && dotnet test`
- Design: [docs/superpowers/specs/2026-05-22-coffee-machine-api-design.md](docs/superpowers/specs/2026-05-22-coffee-machine-api-design.md)

The .docx assessment also has an "Extra Credit" weather-aware variant; that will live on the branch `feat/coffee-weather`.
```

- [ ] **Step 3: Commit**

```bash
git add README.md
git commit -m "docs: root README covers both Customer Onboarding and Coffee Machine projects"
```

---

## Done

All spec requirements covered:

| Spec requirement | Task |
|---|---|
| `GET /brew-coffee` returns 200 + JSON | 6 (BrewService) + 7 (endpoint) + 8 (integration test) |
| Every 5th call returns 503 (empty body) | 5 (counter) + 6 + 7 + 8 |
| April 1st returns 418 (empty body), overrides #2 | 6 + 7 + 8 |
| .NET (latest stable) | 2 |
| Unit + integration tests | 6 (unit) + 8 (integration) |
| Self-contained `coffee-machine/` folder | 2 |
| README with run instructions | 10 |
| Repo root README updated | 11 |
| Stale `feat/bonus-extras` branch removed | 1 |
| Extra-credit weather variant deferred to separate branch | Out of scope (see [spec §9](../specs/2026-05-22-coffee-machine-api-design.md#9-extra-credit--separate-branch)) |
