# PantryChef

![CI](https://github.com/shakybone5dloc/PantryChef/actions/workflows/ci.yml/badge.svg)

An ASP.NET Core (.NET 10) API that tracks what's in your pantry and uses AI to suggest recipes,
prioritizing ingredients that are about to expire.

## Highlights

- **Layered architecture:** Domain / Application / Infrastructure / Api, with dependencies pointing inward
- **EF Core + PostgreSQL:** rich domain model, migrations, per-user data isolation via global query filters
- **AI:** Microsoft.Extensions.AI + Ollama, structured JSON output, retries/timeouts/circuit breaker
- **Auth:** ASP.NET Core Identity + JWT bearer tokens
- **Background work:** daily expiry digest via `BackgroundService` + `PeriodicTimer`
- **Observability:** OpenTelemetry traces, metrics, and logs (Aspire dashboard)
- **Testing:** xUnit v3, `WebApplicationFactory`, Testcontainers, fake AI client and clock
- **Shipping:** multi-stage Dockerfile, EF migrations bundle, docker-compose, GitHub Actions CI

## Run it

```bash
# create .env with POSTGRES_PASSWORD and JWT_SIGNING_KEY (at least 32 characters)
docker compose up --build
```

API: http://localhost:8080 · Telemetry dashboard: http://localhost:18888
