# Technical Assessment

This repository contains solutions for two separate developer technical assessments, both in `./test/`.

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
