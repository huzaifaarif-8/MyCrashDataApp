# CrashDataApp

Full-stack airplane crash analytics dashboard with JWT authentication. The .NET 8 Web API serves both the REST API and the Angular 21 frontend from the same origin on port 5050.

## Architecture

```
Browser → http://localhost:5050
          ├── /           → Angular SPA (served from wwwroot)
          ├── /api/auth   → Auth endpoints (login)
          ├── /api/users  → User management (requires JWT)
          ├── /api/crashes → Crash data endpoints (requires JWT)
          └── /swagger     → Swagger UI
```

**Backend:** ASP.NET Core 8 Web API with Entity Framework Core and SQLite. JWT Bearer authentication via `Microsoft.AspNetCore.Authentication.JwtBearer`. Passwords hashed with BCrypt. Structured logging via Serilog.  
**Frontend:** Angular 21 standalone app with routing, HTTP interceptor for token injection, and route guards. Built into `wwwroot` so both run on the same port.  
**Data:** ~5,268 rows loaded from a CSV file into SQLite on first run via EF Core + CsvHelper.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org) (only needed to rebuild the frontend)

## Running the app

The frontend is pre-built into `wwwroot`. To start the server:

```bash
cd CrashDataApp
dotnet restore
dotnet run --urls http://localhost:5050
```

Open `http://localhost:5050`. On first run EF Core creates `crashes.db`, seeds all crash rows from the CSV, creates the `Users` table, and seeds a default admin account. Subsequent runs reuse the existing database.

To force a re-import, delete `crashes.db` and run again.

## Authentication

The app uses JWT Bearer tokens with an 8-hour expiry.

**Default credentials**

| Username | Password  |
|----------|-----------|
| `admin`  | `Admin@123` |

Change these in `appsettings.json` before first run (the `DefaultAdmin` section). The JWT signing key (`Jwt:Key`) should also be changed to a private value before deploying to production.

After login the Angular app stores the token in `localStorage` and attaches it automatically to every API request via an HTTP interceptor. Visiting any protected route while unauthenticated redirects to `/login`.

## User management

Navigate to **Users** (header link, visible after login) to:

- **Add a user** — enter username and password; duplicate usernames are rejected.
- **Delete a user** — click Delete on any row; deleting the last remaining user is blocked.

The `/users` page maps to `GET /api/users` and `POST /api/users` on the backend (both require a valid JWT).

## Project structure

```
├── CrashDataApp/                   # .NET 8 Web API (backend)
│   ├── Controllers/
│   │   ├── AuthController.cs       # POST /api/auth/login
│   │   ├── UsersController.cs      # GET/POST/DELETE /api/users
│   │   └── CrashesController.cs    # 12 crash data endpoints (all require JWT)
│   ├── Services/
│   │   ├── ICrashService.cs / CrashService.cs
│   │   ├── IUserService.cs / UserService.cs
│   │   └── IAuthService.cs / AuthService.cs
│   ├── Repositories/
│   │   ├── ICrashRepository.cs / CrashRepository.cs
│   │   └── IUserRepository.cs / UserRepository.cs
│   ├── DTOs/
│   │   ├── CrashStatsDtos.cs
│   │   ├── AuthAndUserDtos.cs
│   │   └── PagedResult.cs
│   ├── Validators/
│   │   ├── LoginRequestValidator.cs
│   │   ├── PaginationQueryValidator.cs
│   │   └── ValidationFilter.cs
│   ├── Data/
│   │   ├── CrashContext.cs         # EF Core DbContext (Crashes + Users tables)
│   │   ├── CsvImporter.cs          # seeds SQLite from CSV on first run
│   │   └── Airplane_Crashes_and_Fatalities_Since_1908.csv
│   ├── Models/
│   │   ├── Crash.cs                # crash entity
│   │   ├── AppUser.cs              # user entity (Id, Username, PasswordHash)
│   │   ├── LoginRequest.cs         # login/register DTO
│   │   └── PaginationQuery.cs      # page/pageSize query binding
│   ├── wwwroot/                    # Angular build output (served as static files)
│   ├── Program.cs                  # startup: DI registration, JWT auth, DB seeding, middleware
│   ├── appsettings.json            # connection string, JWT config, default admin
│   └── CrashDataApp.csproj
└── crash-dashboard/                # Angular 21 frontend (source)
    ├── src/app/
    │   ├── components/
    │   │   ├── dashboard/          # main analytics dashboard
    │   │   ├── login/              # login page (/login)
    │   │   └── users/              # user management (/users)
    │   ├── services/
    │   │   ├── auth.service.ts     # login, logout, token storage
    │   │   └── crash-api.service.ts
    │   ├── interceptors/
    │   │   └── auth.interceptor.ts # attaches Bearer token to all requests
    │   ├── guards/
    │   │   └── auth.guard.ts       # redirects to /login if unauthenticated
    │   ├── app.config.ts           # router, HTTP client, interceptor
    │   └── app.ts                  # root shell (router-outlet)
    ├── angular.json                # builds output into CrashDataApp/wwwroot
    └── package.json
```

## API endpoints

### Auth

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/auth/login` | None | Returns a JWT token |

### Users

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/api/users` | JWT | List all users (id + username) |
| POST | `/api/users` | JWT | Create a new user |
| DELETE | `/api/users/{id}` | JWT | Delete a user |

### Crash data

All endpoints are under `/api/crashes` and require a valid JWT.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | All crashes (paginated) |
| GET | `/{id}` | Single crash by ID |
| GET | `/summary` | Total crashes, fatalities, aboard, fatality rate |
| GET | `/by-decade` | Crashes and fatalities grouped by decade |
| GET | `/top-operators` | Top 10 operators by fatalities |
| GET | `/military-vs-civilian` | Fatality split by category |
| GET | `/top-aircraft-types` | Top 8 aircraft types by crash count |
| GET | `/engine-failure` | Years with most engine-failure mentions |
| GET | `/cumulative-fatalities` | Running fatality total over time |
| GET | `/year-over-year` | YoY % change in crash count (last 10 years) |
| GET | `/top-regions` | Top 10 regions by fatalities |
| GET | `/deadliest-per-decade` | Worst single crash in each decade |

Interactive docs (with Bearer token support) at `http://localhost:5050/swagger`.

## Logging

The app uses [Serilog](https://serilog.net) for structured logging. Every log entry is a set of named fields, not a plain string, so you can filter and query by specific values.

### Where logs go

| Sink | Location | Notes |
|------|----------|-------|
| Console | Terminal output | Visible while the server is running |
| File | `CrashDataApp/logs/crash-api-YYYYMMDD.txt` | Rolls daily, last 7 days kept |

### Viewing logs

**Live in the terminal** — just run the app; every request, login, and error prints to stdout.

**Follow the file in real time:**
```bash
tail -f CrashDataApp/logs/crash-api-$(date +%Y%m%d).txt
```

**Read today's full log:**
```bash
cat CrashDataApp/logs/crash-api-$(date +%Y%m%d).txt
```

### What gets logged

| Event | Level | Fields |
|-------|-------|--------|
| App startup | INF | — |
| Every HTTP request | INF | Method, Path, StatusCode, Duration (ms) |
| Successful login | INF | Username |
| Failed login attempt | WRN | Username, IP address |
| Crash record not found | WRN | Record ID |
| Summary endpoint hit | INF | Total crashes, fatalities, fatality rate |

### Log levels by environment

- **Production** — `Information` and above; Microsoft framework logs suppressed to `Warning`
- **Development** — `Debug` and above; EF Core SQL commands visible at `Information`

Change the minimum level in `appsettings.json` under the `Serilog.MinimumLevel` key without recompiling.

## Layered architecture

[#layered-architecture](#layered-architecture)

The backend is split into four layers, each only depending on the one below it:

```
Controllers  →  Services  →  Repositories  →  Data (EF Core / CrashContext)
```

- **Controllers** (`Controllers/`) parse the request, call one service method, and shape the HTTP response.
- **Services** (`Services/`) hold the business logic: `CrashService` does the stats/aggregation, `UserService` handles create/delete rules, `AuthService` verifies credentials and issues JWTs.
- **Repositories** (`Repositories/`) are the only layer that talks to `CrashContext` / EF Core.
- **DTOs** (`DTOs/`) are the typed shapes returned across layer boundaries instead of anonymous objects.

All four are registered as `Scoped` services in `Program.cs`.

## Request validation

[#request-validation](#request-validation)

Input validation is handled by [FluentValidation](https://docs.fluentvalidation.net/) rather than data-annotation attributes.

- `Validators/LoginRequestValidator.cs` validates `LoginRequest` (used by both `POST /api/auth/login` and `POST /api/users`).
- `Validators/PaginationQueryValidator.cs` validates `page`/`pageSize` on `GET /api/crashes`.
- `Validators/ValidationFilter.cs` is an `IAsyncActionFilter` that finds a registered `IValidator<T>` for each action argument and returns `400` with `ValidationProblemDetails` on failure.

Validators are auto-discovered via `AddValidatorsFromAssemblyContaining<Program>()` in `Program.cs`.

## How Entity Framework Core works here

1. `Crash.cs` and `AppUser.cs` define the C# models — EF maps each property to a database column.
2. `CrashContext.cs` registers both tables via `DbSet<Crash>` and `DbSet<AppUser>`.
3. On startup, `EnsureCreated()` creates `crashes.db` (and the `Crashes` table) if it does not exist. The `Users` table is created via a `CREATE TABLE IF NOT EXISTS` raw SQL call so it is safely added to existing databases too.
4. `CsvImporter.SeedIfEmpty()` checks whether `Crashes` is empty; if so, it reads every row from the CSV with CsvHelper and bulk-inserts them in batches of 500.
5. If `Users` is empty a default admin is seeded using the credentials from `appsettings.json`.
6. Controller endpoints query the database with LINQ — EF translates the LINQ expressions to SQL at runtime.

## Rebuilding the frontend

If you edit anything in `crash-dashboard/src/`:

```bash
cd crash-dashboard
npm install
npx @angular/cli@21 build --configuration development
```

The build output goes directly into `CrashDataApp/wwwroot` (configured in `angular.json`). Restart the .NET server to serve the updated files.

For live development with hot reload, run the Angular dev server separately:

```bash
# Terminal 1 — backend
cd CrashDataApp
dotnet run --urls http://localhost:5050

# Terminal 2 — frontend dev server (proxies /api to port 5050)
cd crash-dashboard
npm install
npx ng serve --port 4200
```

Then open `http://localhost:4200`.
