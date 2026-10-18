# Tekus — Provider Services

Full-stack technical test: a **supplier management platform** where a company registers its
providers, publishes the catalogue of services each one offers, and reports on that portfolio by
country.

In plain terms, the application answers three questions:

| Question | Where it is answered |
|----------|----------------------|
| *Who are our suppliers and what do they offer?* | **Providers** page + `GET /api/v1/providers` |
| *What services exist, at what hourly rate, and where are they available?* | **Services** page + country coverage per service |
| *What does the portfolio look like as a whole?* | **Dashboard** (`GET /api/v1/dashboard/summary`) |

A provider carries its own **user-defined custom fields** (the classic *"Número de contacto en
marte"*), so the schema does not force a fixed set of attributes on every supplier. The countries
served by each service are **not** a static table: they come from an external catalogue
([REST Countries](https://restcountries.com/)) that the API snapshots into the database on first
access, so ISO codes and regions stay consistent without hand maintenance.

---

## What you can do in the application

**Authentication**
- Sign in with e-mail and password; the API issues a JWT access token (60 min) plus a refresh
  token that is **rotated** on every use and stored only as a SHA-256 hash.
- The SPA restores the session from the stored token and forces a new sign-in when it expires.
- Two roles: **Admin** (every write operation) and **Analyst** (read only).

**Providers**
- Create, edit and soft-delete suppliers (NIT, name, contact e-mail, active flag).
- Manage **custom fields** per provider: add, edit and remove them by name, up to 50 per provider.
- Manage the **service portfolio**: choose which catalogue services the provider offers. The whole
  set is replaced in one call (`PUT /api/v1/providers/{id}/services`).
- Search by name, NIT or e-mail; sort by any column; paginate.

**Services**
- Create and edit catalogue entries: name, description and hourly rate in USD.
- Activate / deactivate a service without deleting it (history keeps reporting correctly).
- Set the **countries where the service is available**, replacing the whole coverage list.
- Search, sort and paginate every list.

**Countries**
- Paginated, searchable catalogue of ISO countries, refreshed from the external service on demand
  (`POST /api/v1/countries/refresh`) or automatically when the local snapshot is empty.
- If the external service is unreachable, the API keeps serving the last snapshot instead of
  failing the request.

**Dashboard**
- Totals (providers, active providers, services, active services, countries, offerings).
- Services offered **per country** with the number of providers behind each figure.
- **Average, minimum and maximum hourly rate per country**, and the top providers by number of
  services.

**Cross-cutting behaviour**
- Pagination, ordering and free-text search on **every** list endpoint.
- **Optimistic concurrency**: aggregates carry a `Version`; writes accept an `If-Match` header and
  answer `409 Conflict` when someone else changed the record first. Omitting `If-Match` skips the
  check.
- Soft delete everywhere — records are deactivated, never physically removed, so historical
  reporting stays intact.
- Validation on every input (FluentValidation) with `400` + field-level messages, RFC 7807
  `ProblemDetails` errors, rate limiting on the sign-in endpoint, health checks, structured
  request logging (Serilog) and an OpenAPI/Swagger UI surface.

---

## Architecture

Clean Architecture + DDD, four projects in `src/`:

```
src/
├── Tekus.Domain          Aggregates (Provider, Service, Country, User), domain events, guards
├── Tekus.Application     CQRS use cases: queries/commands/handlers, DTOs, validators, abstractions
├── Tekus.Infrastructure  EF Core, JWT, external country catalogue, hashing, persistence
└── Tekus.Api             Controllers, middleware, DI composition, Swagger
```

- The **Provider** and **Service** aggregates are separate boundaries: a provider references
  services **by id**, never by object graph, so two suppliers can offer the same service without
  sharing mutable state. The join table (`ProviderServices`) belongs to the Provider aggregate and
  carries `AssignedAtUtc`, which is what makes the dashboard figures explainable over time.
- Commands and queries go through a small in-house dispatcher (`ISender`) with a pipeline of
  behaviours: request logging → validation.
- `TimeProvider` is injected everywhere, so nothing reads `DateTime.UtcNow` directly.

**Frontend** (`frontend/`, Vite + React 18 + TypeScript)

```
frontend/src/
├── api/          One module per resource; contract mapping lives here (e.g. page → pageNumber)
├── auth/         Token storage, JWT claim decoding, protected routes
├── components/   Modal, Pagination, TableControls, shared UI primitives
├── lib/          Axios instance, 401 handling, error message extraction
└── pages/        Dashboard, Providers, Services, Countries, Login
```

State and server cache are handled by **TanStack Query**; routing by **React Router**; styling by
**Tailwind CSS v4** with a "corporate report" theme (Source Serif 4 + Inter).

---

## Prerequisites

- .NET SDK 8.0 or newer
- Node.js 20 or newer
- SQL Server: LocalDB, SQLEXPRESS or a full instance

---

## 1. Database

The deliverable scripts live in [`db/`](db/) and are idempotent, so re-running any of them is
harmless:

| File | Purpose |
|------|---------|
| `00_create_database.sql` | Creates the `TekusProviderServices` database |
| `01_schema.sql` | Tables, indexes, foreign keys (generated from the EF Core model) |
| `02_seed_data.sql` | ≥ 10 rows per table, including the demo users |

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i db/00_create_database.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i db/01_schema.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i db/02_seed_data.sql
```

Full details (seed contents, password hashing, how to regenerate the schema) in
[`db/README.md`](db/README.md).

> **Point the API at your server.** Edit `ConnectionStrings:Default` in
> `src/Tekus.Api/appsettings.json` — the value shipped in the repository references a specific
> machine instance, so it must be changed the first time you clone the project.
>
> Running the API in the **Development** environment also applies pending EF Core migrations at
> startup and seeds the demo data automatically when the tables are empty, so an empty database
> works too.

---

## 2. Backend

**Visual Studio** — open `Tekus.ProviderServices.sln`, set `Tekus.Api` as the startup project and
press F5 with the `http` profile. Swagger opens at <http://localhost:5199/swagger>.

**Command line**

```bash
dotnet restore
dotnet build Tekus.ProviderServices.sln
dotnet run --project src/Tekus.Api
```

Every launch profile listens on **http://localhost:5199**, which is the address the frontend proxy
expects.

---

## 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Open <http://localhost:5173>. Vite proxies `/api` to `http://localhost:5199`, so no CORS
configuration is needed during development.

Production build:

```bash
cd frontend
npm run build     # output in frontend/dist
```

---

## Demo credentials

Seeded users share one password:

| E-mail | Role |
|--------|------|
| `admin@tekus.co` | Admin |
| `analyst@tekus.co` | Analyst |

| Password |
|----------|
| `Tekus.2026!` |

---

## API at a glance

Base path: `/api/v1`. All lists accept `pageNumber`, `pageSize`, `search`, `sortBy` and
`sortDirection`; `pageSize` is capped at 100.

| Method | Route | Notes |
|--------|-------|-------|
| POST | `/auth/login` | Access + refresh token pair (rate limited) |
| POST | `/auth/refresh` | Rotates the refresh token |
| POST | `/auth/logout` | Revokes a refresh token (idempotent) |
| GET | `/auth/me` | Identity behind the current token |
| GET / POST | `/providers` | Paginated list / create |
| GET / PUT / PATCH / DELETE | `/providers/{id}` | Detail, edit, activate, soft delete |
| PUT | `/providers/{id}/custom-fields/{key}` | Create or update a custom field |
| DELETE | `/providers/{id}/custom-fields/{key}` | Remove a custom field |
| PUT | `/providers/{id}/services` | Replace the whole service portfolio |
| GET / POST | `/services` | Paginated list / create |
| GET / PUT / PATCH / DELETE | `/services/{id}` | Detail, edit, activate, soft delete |
| PUT | `/services/{id}/countries` | Replace the countries where it is available |
| GET | `/countries` | Paginated country catalogue |
| POST | `/countries/refresh` | Force a resync with the external service |
| GET | `/dashboard/summary` | Totals, per-country figures, top providers |
| GET | `/health` | Health check |

Custom fields are addressed **by key** (there is no field id and no collection endpoint): read them
from `GET /providers/{id}` → `customFields`, upsert with `PUT .../custom-fields/{key}` and a body of
`{ "value": "..." }`.

---

## Tests

The three test projects are scaffolded for **xUnit** with FluentAssertions, Moq and Bogus, and the
integration project is prepared to run against **SQLite in-memory** so it needs no live server or
database. Test cases are still to be written — `dotnet test` currently reports none.

---

## Repository layout

```
├── db/            SQL Server scripts (schema + seed) — the database deliverable
├── frontend/      React + Vite SPA
├── src/           The four backend projects
└── tests/         Unit and integration test projects
```
