# Book Manager

A book catalogue that records every change made to a book and shows that history as a filterable, sortable, paginated activity feed, with live updates pushed to connected clients.

ASP.NET Core (Clean Architecture + CQRS) on PostgreSQL, React 19 + Mantine on the frontend, SignalR between them.

## Run with Docker

Docker Desktop, or Docker Engine with Compose v2, is the only prerequisite. From the repository root:

```bash
docker compose up --build
```

| | URL |
| --- | --- |
| Frontend | http://localhost:3000 |
| API | http://localhost:5138 |
| Swagger UI | http://localhost:5138/swagger |

Compose starts PostgreSQL, applies the migrations, seeds three sample authors, and serves the app. No .NET SDK, Node.js, PostgreSQL install, or user secrets needed.

```bash
docker compose down
```

```bash
docker compose down -v
```

The first stops the stack and keeps your data; the second also deletes the database volume for a clean slate.

## Run locally

| Requirement | Version |
| --- | --- |
| .NET SDK | 10.0 |
| Node.js | 20.19+ or 22.12+ |
| PostgreSQL | 14+ |
| `dotnet-ef` | `dotnet tool install --global dotnet-ef` |

**1.** Start PostgreSQL and create a `bookmanager` database.

**2.** Set the connection string — it is not committed:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=bookmanager;Username=postgres;Password=postgres" --project backend/BookManager.Api
```

**3.** Apply the migrations:

```bash
dotnet ef database update --project backend/BookManager.Infrastructure
```

**4.** Set up the frontend:

```bash
cp client/.env.example client/.env
```

```bash
npm install --prefix client
```

**5.** Run the API and the client in separate terminals:

```bash
dotnet run --project backend/BookManager.Api
```

```bash
npm run dev --prefix client
```

The frontend is then on http://localhost:5173 and the API on http://localhost:5138. Unlike the Docker setup, a local run starts with an empty database.

## What it does

- **Books** — list, view, create, and edit books (title, short description, publish date, authors).
- **Change history** — every create and edit is persisted as an immutable `BookEvent`. The Events page pages, filters by book and change type, and sorts by time or book title; filtering by one book gives that book's full history.
- **Live updates** — changes are pushed over SignalR to every open client: lists refresh themselves, a toast appears, and the change lands in the notification bell.
- **Catch-up** — each book tracks a per-viewer read watermark, so reopening a book shows a dialog summarising what changed since your last visit.

## How it works

Changes are captured in the domain rather than by diffing rows afterwards. `Book.UpdateDetails` records a `BookChange` only when a value actually differs. [BookEventInterceptor](backend/BookManager.Infrastructure/Persistence/Interceptors/BookEventInterceptor.cs), an EF Core `SaveChanges` interceptor, writes those changes into `BookEvents` inside the same transaction as the book update, so a change is never recorded without its edit. [BookEventDispatcher](backend/BookManager.Infrastructure/Events/BookEventDispatcher.cs) then drains that outbox in the background, claiming rows with `FOR UPDATE SKIP LOCKED` so multiple API instances never publish the same event twice.

The history is therefore a durable append-only log in PostgreSQL, with SignalR only as the delivery mechanism on top of it.

On the frontend, the API types are generated from the OpenAPI document, so a backend contract change breaks the build rather than production.

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

SignalR hub: `/hubs/book-events`, broadcasting `BookEventCreated`. Validation failures and unhandled exceptions are returned as RFC 7807 `ProblemDetails`.

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
docker/                        Sample data applied by the seeder service
docker-compose.yml             postgres -> migrator -> seeder -> api -> client
```

The frontend is organised by feature, with each feature owning its React Query hooks. Shared UI lives in `components/` and is presentation-only.
