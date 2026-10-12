# BoaSorte API

.NET 10 Minimal API in Clean Architecture. Specs, plans and the constitution live in the `boasorte-app` repo under `specs/`.

```
src/    BoaSorte.Domain → BoaSorte.Application → BoaSorte.Infrastructure / BoaSorte.Api
tests/  Domain · Application · Api.IntegrationTests (Testcontainers) · Architecture (layer rules)
```

## Development

Requirements: .NET SDK 10, Docker.

```bash
# Connection string (kept out of git)
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=boasorte;Username=<user>;Password=<password>" \
  --project src/BoaSorte.Api

dotnet run --project src/BoaSorte.Api      # http://localhost:5236  (GET /api/health)
dotnet test                                # integration tests start their own postgres:18 container
```

No local Postgres? `docker compose -f deploy/docker-compose.dev.yml up -d` in the `boasorte-app` repo (port 5433).

## Configuration

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__Default` | PostgreSQL connection string (required) |
| `ASPNETCORE_ENVIRONMENT` | `Production` on the VPS |

The container listens on `8080`; TLS terminates at Traefik (Dokploy). Build: `docker build -t boasorte-api .` from the repo root.
