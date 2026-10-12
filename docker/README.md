# Running HelpDesk with Docker

Works the same on Windows, macOS and Linux. The only requirement is Docker with Compose v2.

```sh
cp .env.example .env      # set MSSQL_SA_PASSWORD (and HELPDESK_PORT if 8080 is taken)
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

## Notes

- SQL Server images are amd64 only. On Apple Silicon, Docker Desktop runs them through emulation; enable
  "Use Rosetta for x86_64/amd64 emulation" for best results.
- `docker/db/01-schema.sql` is the root `script.sql` converted to UTF-8 without Windows file paths.
- Shell and SQL files under `docker/` are forced to LF line endings by `.gitattributes`.
