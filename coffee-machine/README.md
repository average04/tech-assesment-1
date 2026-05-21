# Coffee Machine API

Minimal API satisfying the .docx assessment in `../test/Developer Technical Test..docx`.

> **Branch note.** You are reading the README on whichever branch is currently checked out.
> - `main` — basic build covering the three required behaviors (200/503/418).
> - `feat/coffee-weather` — extra-credit variant: adds weather-aware messages (>30°C → iced).

## Behaviors
- `GET /brew-coffee` → 200 OK with `{ "message": "Your piping hot coffee is ready", "prepared": "<ISO-8601>" }`
- Every 5th call → 503 Service Unavailable (empty body)
- On April 1st → 418 I'm a teapot (empty body) — overrides the 5th-call rule
- **On `feat/coffee-weather` only:** if the configured weather service reports current temperature > 30°C, the 200 OK message becomes `"Your refreshing iced coffee is ready"`. April 1st (418) and every-5th (503) rules are unchanged and take precedence.

## Stack
.NET 10 Minimal API, `TimeProvider` for testable clock, `WebApplicationFactory` for integration tests, FluentAssertions 6.x. On `feat/coffee-weather`: `IHttpClientFactory` + OpenWeatherMap.

## Run

```bash
cd coffee-machine
dotnet run --project src/Api
# API: http://localhost:5050

curl http://localhost:5050/brew-coffee
```

### Optional: enable the weather check (`feat/coffee-weather` only)

The weather service is **off by default**: with no API key configured, `IWeatherService` returns `null` and the endpoint falls back to the hot message. To enable the iced behavior, set an OpenWeatherMap API key in either:

- `coffee-machine/src/Api/appsettings.json` (or `appsettings.Development.json`):
  ```json
  "Weather": {
    "ApiKey": "your-openweathermap-key",
    "City": "London",
    "Units": "metric"
  }
  ```
- Or environment variable (overrides appsettings):
  ```bash
  Weather__ApiKey=your-openweathermap-key dotnet run --project src/Api
  ```

If the weather call fails (bad key, network, parse error), the endpoint logs a warning and falls back to the hot message — it never 500s on a weather failure.

## Test

```bash
cd coffee-machine
dotnet test
```

Expected:
- On `main`: 18 tests pass (13 unit + 5 integration).
- On `feat/coffee-weather`: 33 tests pass (23 unit + 10 integration).

## Design notes
See [../docs/superpowers/specs/2026-05-22-coffee-machine-api-design.md](../docs/superpowers/specs/2026-05-22-coffee-machine-api-design.md).

The .docx submission instructions say: *"please supply the solution separately to the original – for instance, as a second Zip file, or as a Git branch."* That separation is the `feat/coffee-weather` branch.

To see the extra-credit diff cleanly:
```bash
git diff main..feat/coffee-weather -- coffee-machine/
```
