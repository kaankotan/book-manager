# Book Manager

A book catalogue that records every change made to a book and surfaces that history as a filterable, sortable, paginated activity feed — with live updates pushed to connected clients.

ASP.NET Core (Clean Architecture + CQRS) on PostgreSQL, React 19 + Mantine on the frontend, SignalR between them.

## Features

- **Books** — list, view, create, and edit books (title, short description, publish date, authors).
- **Change history** — every create and edit is persisted as an immutable `BookEvent`. The Events page supports pagination, filtering by book and by change type, and sorting by time or book title. Filtering by a single book gives that book's full history.
- **Live updates** — changes are pushed over SignalR to every open client: the activity feed and book list refresh themselves, a toast appears, and the change lands in the notification bell inbox.
- **Catch-up on changes you missed** — each book tracks a per-viewer read watermark. Reopening a book you have seen before shows a dialog summarising what changed since your last visit.
- **Typed API client** — the frontend's types are generated from the API's OpenAPI document, so a backend contract change breaks the frontend build rather than production.

## How change tracking works

Changes are captured in the domain, not by comparing rows after the fact:

1. `Book.UpdateDetails` records a `BookChange` on the aggregate — but only when a value actually differs.
2. `BookEventInterceptor`, an EF Core `SaveChanges` interceptor, drains those pending changes into `BookEvents` rows **inside the same transaction** as the book update. A change is never recorded without its edit, and vice versa (transactional outbox).
3. `BookEventDispatcher`, a hosted background service, polls the outbox and claims undispatched rows with `FOR UPDATE SKIP LOCKED`, so multiple API instances can drain it concurrently without double-publishing. Each claimed event is pushed to SignalR clients and stamped as dispatched.

The history is therefore a durable append-only log in PostgreSQL; SignalR is only the delivery mechanism on top of it.

## Quick start with Docker

The only prerequisite is Docker Desktop, or Docker Engine with Compose v2. From the repository root:

```bash
docker compose up --build
```

That builds both images, starts PostgreSQL, applies the migrations, and serves the app:

| | URL |
| --- | --- |
| Frontend | http://localhost:3000 |
| API | http://localhost:5138 |
| Swagger UI | http://localhost:5138/swagger |

No .NET SDK, Node.js, PostgreSQL installation, or user secrets required — the containers receive the connection string as an environment variable.

Migrations are applied by a dedicated one-shot `migrator` service rather than by the API itself. It waits for PostgreSQL to pass its healthcheck, runs `dotnet ef database update`, and exits; only then does the API start. The API never alters its own schema.

Stop the stack:

```bash
docker compose down
```

Stop it and delete the database volume, for a clean slate:

```bash
docker compose down -v
```

PostgreSQL's port is deliberately not published, so it cannot collide with an instance you already run locally. Add a `ports` mapping to the `postgres` service in [docker-compose.yml](docker-compose.yml) if you want to connect to it directly.

## Running locally without Docker

### Prerequisites

| Requirement | Version |
| --- | --- |
| .NET SDK | 10.0 |
| Node.js | 20.19+ or 22.12+ (required by Vite 8) |
| PostgreSQL | 14+ |
| `dotnet-ef` | Install with `dotnet tool install --global dotnet-ef` |

### 1. Start PostgreSQL

Any PostgreSQL instance works. With Docker:

```bash
docker run --name bookmanager-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=bookmanager -p 5432:5432 -d postgres:17
```

### 2. Configure the connection string

The connection string is not committed. Set it with user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=bookmanager;Username=postgres;Password=postgres" --project backend/BookManager.Api
```

`BookManager.Api` and `BookManager.Infrastructure` share a `UserSecretsId`, so this one secret serves both the running API and EF Core's design-time tooling.

### 3. Apply migrations

```bash
dotnet ef database update --project backend/BookManager.Infrastructure
```

### 4. Configure and install the frontend

Copy the example environment file — the defaults match the API's development URL, so no edits are needed:

```bash
cp client/.env.example client/.env
```

Then install dependencies:

```bash
npm install --prefix client
```

### 5. Run

Two processes, in separate terminals:

```bash
dotnet run --project backend/BookManager.Api
```

```bash
npm run dev --prefix client
```

| | URL |
| --- | --- |
| Frontend | http://localhost:5173 |
| API | http://localhost:5138 |
| Swagger UI | http://localhost:5138/swagger |

The API's CORS policy allows `http://localhost:5173` and `http://localhost:3000` in development; add your origin to `Cors:AllowedOrigins` in `appsettings.Development.json` if you serve the client elsewhere.

## API

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/books` | All books |
| `GET` | `/api/books/{id}` | A single book |
| `POST` | `/api/books` | Create a book |
| `PUT` | `/api/books/{id}` | Update a book's title and description |
| `GET` | `/api/authors` | All authors |
| `GET` | `/api/authors/{id}` | A single author |
| `POST` | `/api/authors` | Create an author |
| `GET` | `/api/events` | Change history — paged, filterable by `bookIds` and `changeTypes`, sortable by `sortBy` and `descending` |
| `GET` | `/api/books/{bookId}/events` | Change history scoped to one book |
| `GET` | `/api/books/{bookId}/unseen-changes` | Changes since the viewer's last visit |
| `PUT` | `/api/books/{bookId}/view` | Advance the viewer's read watermark |

SignalR hub: `/hubs/book-events`, broadcasting `BookEventCreated`.

Validation failures and unhandled exceptions are returned as RFC 7807 `ProblemDetails` via a global exception handler.

## Tests

```bash
dotnet test
```

Covers the domain model, command and query handlers, validators, mapping profiles, and the MediatR validation behavior.

## Project structure

```
backend/
  BookManager.Domain/          Entities and invariants — no dependencies
  BookManager.Application/     CQRS handlers, validators, DTOs, repository interfaces
  BookManager.Infrastructure/  EF Core, migrations, repositories, outbox dispatcher
  BookManager.Api/             Controllers, SignalR hub, exception handling, Swagger
  BookManager.Tests/           xUnit + NSubstitute
client/
  src/api/                     Generated OpenAPI types and typed fetch client
  src/components/              Reusable UI (EditableText, StatePanel, Header, ...)
  src/features/                books, authors, events
  src/realtime/                SignalR connection, event provider, status badge
docker-compose.yml             postgres -> migrator -> api -> client
```

The frontend is organised by feature, with each feature owning its React Query hooks. Shared UI lives in `components/` and is presentation-only.

## Tooling

```bash
dotnet tool restore              # csharpier, reportgenerator
dotnet csharpier format .        # format C#
npm run lint --prefix client     # oxlint
npm run format --prefix client   # prettier
npm run typecheck --prefix client
npm run generate:api --prefix client   # regenerate API types (API must be running)
```
