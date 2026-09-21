# Customer Loyalty Campaign

A solution for a telecom loyalty campaign: customer service agents register eligible customers for a discount (max 5 per agent per day, for one week), a monthly purchase report is merged into the system, and campaign results are exposed through a secured API that other CRM systems can integrate with.

Built for the Comtrade System Integration recruitment task.

## Architecture

```
                ┌──────────────────┐
  Agent/Admin → │  Web Portal      │  ASP.NET Core MVC, cookie auth
   (browser)    │  container :8081 │
                └────────┬─────────┘
                         │ HTTP + Bearer JWT
                         ▼
 External CRM ──→ ┌──────────────────┐  SOAP   ┌──────────────────┐
 (M2M, JWT)       │  Campaign API    │───────→ │ FindPerson (CRM) │
                  │  container :8080 │         │ crcind.com       │
                  └────────┬─────────┘         └──────────────────┘
                           │ EF Core (connection string from environment)
                           ▼
                  ┌──────────────────┐
                  │  MySQL 8         │  named volume + backup script
                  │  container :3307 │
                  └──────────────────┘
```

Three independent components, each in its own container:

- **Web Portal** has no reference to any backend project. It talks to the API over HTTP only, so it can be deployed, scaled and replaced independently.
- **Campaign API** holds all business logic and is the single entry point for both the portal and external systems.
- **MySQL** is the default database, but the backend is database-agnostic (see below).

### Solution structure (Clean Architecture)

```
src/
├─ CustomerCampaign.Domain          entities, enums, invariants (no dependencies)
├─ CustomerCampaign.Application     use cases, business rules, ports (interfaces), DTOs
├─ CustomerCampaign.Infrastructure  EF Core, SOAP adapter, JWT, CSV parser, clock
├─ CustomerCampaign.Api             controllers, authentication, error handling, DI root
└─ CustomerCampaign.Portal          MVC web portal (independent service)
```

Dependencies point inward: `Api → Infrastructure → Application → Domain`. Changing the database, the CRM or the identity provider is a change in Infrastructure only.

## Tech stack

.NET 10, ASP.NET Core Web API and MVC, Entity Framework Core 9 (Pomelo MySQL, SQL Server, Npgsql providers), JWT Bearer authentication, Microsoft.Extensions.Http.Resilience (Polly), CsvHelper, Swagger, Docker and Docker Compose.

EF Core 9 is used deliberately: it runs on .NET 10, and the Pomelo MySQL provider had no stable EF Core 10 release at the time of development. Upgrading is a version change in one `.csproj` file.

## Running the solution

### With Docker (recommended)

```bash
docker compose up -d --build
```

| Component | URL |
|---|---|
| Web portal | http://localhost:8081 |
| API health check | http://localhost:8080/health |
| MySQL | localhost:3307 (user `campaign`) |

Stop with `docker compose down` (data kept) or `docker compose down -v` (data removed).

If the public FindPerson demo service is down, run the API with the stub CRM (see [CRM integration](#crm-integration)):

```powershell
$env:CRM_PROVIDER = "Stub"; docker compose up -d
```

### Locally (Visual Studio)

1. Set the connection string and JWT key with User Secrets (they are never committed):
```bash
   dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Port=3306;Database=customer_campaign;User=root;Password=<your-password>;" --project src/CustomerCampaign.Api
   dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-characters>" --project src/CustomerCampaign.Api
```
2. Set `CustomerCampaign.Api` and `CustomerCampaign.Portal` as multiple startup projects and run.
3. The database, schema and demo data are created automatically on API startup (migrations + seed).
4. Swagger UI is available at `https://localhost:7233/swagger` in Development.

### Demo accounts

| Role | Username | Password |
|---|---|---|
| Admin | `admin` | `Admin123!` |
| Agent | `agent1` | `Agent123!` |
| Agent | `agent2` | `Agent123!` |
| API client (M2M) | `crm-system` | `crm-secret-2026` |

These exist only for the demo. The seeded campaign starts on the day the database is created and lasts 7 days.

## End-to-end process

1. An agent decides which customers are eligible for the discount (outside the system, e.g. by phone).
2. The agent signs in to the portal, checks the customer ID against the CRM and registers the customer. The API validates the customer through the FindPerson service and stores a record with `DiscountOfferAccepted = false`.
3. Each agent can register at most 5 customers per day. Mistakes are corrected by cancelling a registration on the same day, which frees the slot.
4. A parallel sales channel uses the list of eligible customers. One month after the campaign, it provides a CSV report of customers who actually purchased with the discount.
5. An admin uploads the report through the API. Matching records are flagged as `DiscountOfferAccepted = true`.
6. Admins see results in the portal; external CRM systems read them through the API.

## Business rules

| Rule | Implementation |
|---|---|
| Max 5 active registrations per agent per day | Limit is a campaign property (`DailyLimitPerAgent`), not a constant. Cancelled records do not count. |
| Registration only during the campaign period | `Campaign.IsActiveOn(today)` |
| Customer must exist in the CRM | Checked through the `ICustomerDirectory` port before saving |
| A customer can be rewarded only once per campaign | Checked in the service and enforced by a unique index `(CampaignId, CustomerExternalId)` |
| Agents cancel only their own records, only on the same day | Keeps the eligible list stable for the sales channel once a day is closed |
| Import is idempotent and tolerant | Re-importing a file changes nothing; invalid rows are reported, not fatal |
| "Today" is calendar day in `Europe/Belgrade` | `IClock` abstraction; containers run in UTC |

## Assumptions

The task description is intentionally open, so these assumptions were made:

- The FindPerson `id` is the unique customer identifier in the CRM and is also the key used in the purchase report.
- CSV format: `CustomerId,PurchaseDate[,OrderReference]`. Dates are accepted as `yyyy-MM-dd`, `d.M.yyyy` or `M/d/yyyy` (the format Excel produces in an en-US locale). `d/M/yyyy` is intentionally not supported because values like `9/10/2026` would be ambiguous. A sample is in `sample-data/`.
- A purchase dated before the reward date is treated as a data error and reported.
- Agents select customers outside the system; the system records and validates the selection.
- Users and campaigns are provisioned by seed data; user and campaign management screens are out of scope.
- Only a customer snapshot (name, city) is stored. The SSN returned by the CRM is never persisted (data minimisation).

## API

All endpoints are versioned under `/api/v1`. Errors use RFC 7807 Problem Details.

| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/auth/login` | anonymous | Agent/admin login, returns JWT |
| POST | `/auth/token` | anonymous | Machine-to-machine token (client credentials) |
| GET | `/customers/{id}` | Agent, Admin | CRM lookup |
| POST | `/rewards` | Agent | Register an eligible customer |
| GET | `/rewards/my?date=` | Agent | Agent's registrations and remaining daily limit |
| DELETE | `/rewards/{id}` | Agent (owner) | Cancel a registration (soft delete) |
| GET | `/rewards?accepted=&campaignId=&page=` | Admin, Integration | Eligible or purchased customers, paged |
| POST | `/imports/purchases` | Admin | Upload the monthly CSV report |
| GET | `/campaigns/current/results` | Admin, Integration | Campaign results |
| GET | `/campaigns/{id}/results` | Admin, Integration | Results for a specific campaign |
| GET | `/health` | anonymous | Health check |

Status codes: `401` not authenticated, `403` wrong role or not the owner, `404` not found, `422` business rule violation, `503` CRM unavailable.

Example for an external system:

```bash
curl -X POST http://localhost:8080/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"clientId":"crm-system","clientSecret":"crm-secret-2026"}'

curl "http://localhost:8080/api/v1/rewards?accepted=true" \
  -H "Authorization: Bearer <accessToken>"
```

## Key design decisions

**Database independence.** The Application layer depends only on EF Core abstractions. The provider is selected by configuration (`"DatabaseProvider": "MySql" | "SqlServer" | "PostgreSql"`). Migrations are generated for MySQL; other providers create the schema from the model. In production each provider would get its own migrations assembly.

**Configuration from outside.** The connection string and JWT key are never in source code or images. Locally they come from User Secrets, in Docker from environment variables. In Kubernetes they would come from a Secret, without rebuilding the image.

**Authentication.** One JWT infrastructure serves two flows: agents and admins log in with username and password, external systems use client credentials. Because the API validates standard JWTs, moving to Keycloak (or any OpenID Connect provider) means setting `Authority` in the JWT Bearer options; controllers and policies stay unchanged. The portal keeps the JWT inside an HttpOnly, encrypted auth cookie, so it is not reachable from page scripts.

**Resilience.** The CRM call has retries, per-attempt and total timeouts, and a circuit breaker. Database connections retry on transient failures. MySQL's Docker healthcheck uses TCP so the API starts only when the real server is ready.

**Concurrency.** The unique index guarantees that two agents cannot reward the same customer at the same time. The daily limit is checked in the application; under concurrent requests from the same agent it could briefly be exceeded by one, which is acceptable for this use case. A strict guarantee would need a serializable transaction or a per-agent lock.

## CRM integration

FindPerson is accessed through the `ICustomerDirectory` port. `FindPersonSoapClient` builds the SOAP envelope, parses the response and maps it to an internal `CustomerInfo` model. A non-existent customer is an empty `FindPersonResponse`; an unreachable service or SOAP fault becomes HTTP 503.

The public demo service was intermittently unavailable during development, so a second implementation, `StubCustomerDirectory`, is included and selected by configuration:

```json
"CustomerDirectory": { "Provider": "Soap" }
```

`Soap` is the default everywhere. `Stub` is a development aid only (IDs 1 to 200 exist).

Automatic fallback to the stub is deliberately **not** implemented. When the CRM is unavailable, the API returns 503 and the portal tells the agent so, instead of silently storing fabricated customer data.

Supporting another CRM means adding one class that implements `ICustomerDirectory` and registering it; business logic, controllers and the database are unaffected.

## Database backup

```powershell
.\scripts\backup-db.ps1
```

Creates a consistent `mysqldump` snapshot (`--single-transaction`) in `backups/` (git-ignored). The restore command is documented in the script. In production this would run as a scheduled job (Kubernetes CronJob) with off-site storage and retention.

## Git workflow

- `main`: released version (tag `v1.0.0`)
- `develop`: integration branch
- `feature/*`, `docs/*`: one branch per unit of work, merged into `develop` with `--no-ff` so each feature stays visible in the history

## Known limitations and future improvements

- **Identity provider:** replace built-in JWT issuing with Keycloak for SSO, MFA for agents, token revocation and client management.
- **Admin features:** CSV upload in the portal, agent activity per day, correction of older registrations by an admin, user and campaign management.
- **Kubernetes:** Deployments, Services, ConfigMaps and Secrets; readiness and liveness probes; horizontal scaling of the stateless API and portal.
- **Data Protection keys:** persist portal cookie keys outside the container (volume, Redis, Key Vault) so users stay signed in across redeployments.
- **Testing and CI:** unit tests for business rules with a fake `IClock`, integration tests against a MySQL container, GitHub Actions pipeline.
- **Observability:** structured logging (Serilog), OpenTelemetry tracing across portal, API and CRM calls, audit log of agent actions.
- **Performance:** caching of CRM lookups, rate limiting on public endpoints, asynchronous processing of large imports.