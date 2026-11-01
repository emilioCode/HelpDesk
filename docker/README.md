# Running HelpDesk with Docker

Works the same on Windows, macOS and Linux. The only requirement is Docker with Compose v2.

```sh
cp .env.example .env      # set MSSQL_SA_PASSWORD, JWT_KEY and SECRETS_KEY (and HELPDESK_PORT if 8080 is taken)
docker compose up --build
```

Open http://localhost:8080 and sign in with account `root`, password `root`. This demo user comes from
`docker/db/02-seed.sql`; change the password after the first sign-in.

## What starts

| Service   | Purpose |
|-----------|---------|
| `db`      | SQL Server 2022, data kept in the `sqldata` volume |
| `db-init` | Creates `HelpDeskDB` and the demo seed once, then exits. Skips if the database already exists |
| `web`     | ASP.NET Core 6 API serving the Angular SPA on port 8080 |

## Common tasks

```sh
docker build --target tests .   # run the unit tests
docker compose down             # stop, keep data
docker compose down -v          # stop and delete the database
```

## Configuration

The app reads its connection string from `ConnectionStrings__HelpDesk`, set in `docker-compose.yml`.
For another environment (cloud, managed SQL), override that variable; no code change is needed.

### Security settings

| Variable | Purpose |
|----------|---------|
| `JWT_KEY` (`Jwt__Key`) | Signs the access tokens. Required, at least 32 characters, keep it secret. |
| `SECRETS_KEY` (`Secrets__Key`) | Encrypts the companies' mail password in the database. Required, at least 32 characters. Changing it makes the stored mail passwords unreadable. |
| `AUTH_ENFORCE` (`Auth__Enforce`) | `true` (default) rejects requests without a valid token. `false` is a transition mode that lets anonymous requests through; use it only for testing. |
| `JWT_EXPIRES_MINUTES` (`Jwt__ExpiresMinutes`) | Token lifetime, 60 minutes by default. The front end asks for a new token while the user is active. |
| `Auth__MaxFailedAttempts`, `Auth__LockoutMinutes` | Sign-in lockout, 5 attempts and 15 minutes by default. |
| `Cors__AllowedOrigins` | Comma separated origins allowed to call the API from another site. Empty by default because the front end is served by the API itself. |

The app does not start without `Jwt__Key` and `Secrets__Key`. Never reuse the example values.

### Existing databases

The company mail password is stored encrypted and needs a wider column. Databases created before this change must run `db/migrations/001-widen-company-mail-password.sql` once. Plain-text user passwords are converted to hashes the first time each user signs in; a stored mail password is encrypted the next time its company is saved.

## Notes

- SQL Server images are amd64 only. On Apple Silicon, Docker Desktop runs them through emulation; enable
  "Use Rosetta for x86_64/amd64 emulation" for best results.
- `docker/db/01-schema.sql` is the root `script.sql` converted to UTF-8 without Windows file paths.
- Shell and SQL files under `docker/` are forced to LF line endings by `.gitattributes`.
