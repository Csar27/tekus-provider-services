# Database scripts

This folder contains the **deliverable** SQL Server scripts of the challenge. They create the
schema and populate it with enough data to explore the API without writing a single record by hand.

| File | Purpose |
|------|---------|
| `00_create_database.sql` | Creates the `TekusProviderServices` database (idempotent). |
| `01_schema.sql` | Idempotent schema: every table, index and foreign key, plus the `__EFMigrationsHistory` bookkeeping. |
| `02_seed_data.sql` | Initial data set, **≥ 10 rows per table** (idempotent — only fills empty tables). |

## How to run them

Using `sqlcmd` against SQL Server LocalDB (the default in `appsettings.json`):

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i db/00_create_database.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i db/01_schema.sql
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i db/02_seed_data.sql
```

Against a full SQL Server instance, replace the `-S` value with your server and add
`-U <user> -P <password>` if it does not use Windows authentication.

> **Order matters.** Run the scripts in the numeric order above. Each script guards itself
> (`IF DB_ID(...) IS NULL`, `IF NOT EXISTS (SELECT 1 FROM ...)`) so re-running one is harmless.

## What gets seeded

| Table | Rows | Notes |
|-------|-----:|-------|
| `Users` | 10 | `admin@tekus.co` (Admin) and 9 more accounts. |
| `Countries` | 12 | Snapshot of the external catalogue, same codes the API reads. |
| `Services` | 12 | Hourly rates in USD, with descriptions. |
| `ServiceCountries` | 36 | Availability of every service per country. |
| `Providers` | 12 | NIT, name, e-mail — all valid per the domain rules. |
| `ProviderCustomFields` | 14 | User defined fields (e.g. *Número de contacto en marte*). |
| `ProviderServices` | 28 | Which provider offers which service. |
| `RefreshTokens` | 12 | Only SHA-256 hashes (the plaintext never touches the DB). |
| `AuditLogs` | 12 | One row per domain event, showing the audit trail. |

## Demo credentials

Every seeded user shares the same password, which makes it easy to try the API:

| Field | Value |
|-------|-------|
| E-mail | `admin@tekus.co` (role **Admin**) / `analyst@tekus.co` (role **Analyst**) |
| Password | `Tekus.2026!` |

### About the password hash

Passwords are stored as a **self-describing PBKDF2-HMAC-SHA256** string:

```
PBKDF2-SHA256$<iterations>$<saltBase64>$<hashBase64>
```

The value used by this script is produced with a fixed salt (the ASCII of `1234567890abcdef`)
and 100 000 iterations. It is **byte-for-byte identical** to the hash produced by the code
seeder (`Tekus.Infrastructure.Persistence.Seeding.DevelopmentSeeder.DemoPasswordHash`), so a
database seeded by either path accepts the same credentials.

Because the iteration count is embedded in the stored value, it can be raised in a future release
without invalidating existing hashes — old rows keep verifying with the count they carry.

## Relationship to EF Core migrations

`01_schema.sql` is generated from the EF Core model:

```bash
dotnet ef migrations script --idempotent \
    --project src/Tekus.Infrastructure \
    --startup-project src/Tekus.Infrastructure \
    --output db/01_schema.sql
```

then a header (the `USE [TekusProviderServices]` guard) is prepended.

After changing the model, regenerate the migration **and** the script:

```bash
dotnet ef migrations add <Name> \
    --project src/Tekus.Infrastructure \
    --startup-project src/Tekus.Infrastructure \
    --output-dir Persistence/Migrations
```

Alternatively, running the API in the **Development** environment applies pending migrations
automatically at startup and seeds the demo data if the tables are empty — so `dotnet run` alone
is enough to get a populated API.
