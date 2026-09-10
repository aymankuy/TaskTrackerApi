# Task Tracker API

A task/project management system built with ASP.NET Core Web API, Entity Framework Core (Code-First), SQL Server, and JWT authentication. Built as a portfolio project for junior .NET Backend Developer job applications, following a realistic layered architecture (Controller → Service → Repository), DTO usage, JWT-based authentication/authorization, and centralized error handling.

The original requirements document (Turkish) is available at [task-tracker-gereksinimler.md](task-tracker-gereksinimler.md).

## Tech Stack

- **.NET 9** (ASP.NET Core Web API)
- **Entity Framework Core 9** (Code-First, SQL Server provider)
- **SQL Server** (local)
- **JWT Bearer Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **BCrypt.Net-Next** — password hashing
- **Swashbuckle.AspNetCore** (Swagger / OpenAPI)

## Architecture

```
Controller  →  Service  →  Repository  →  ApplicationDbContext (EF Core)  →  SQL Server
```

- **Controller**: handles HTTP concerns only (routing, model binding, responses). No business logic.
- **Service**: business logic and authorization checks live here (e.g. "return 403 if you're not a project member").
- **Repository**: abstracts database access, so the Service layer isn't directly coupled to EF Core.
- **DTOs**: entities are never returned directly from the API (e.g. `User.PasswordHash` never leaks out).
- **Global Exception Middleware**: the Service layer throws `NotFoundException` / `ForbiddenException` / `InvalidOperationException` / `UnauthorizedAccessException`, and `Middleware/GlobalExceptionHandler.cs` centrally maps them to 404/403/400/401 — controllers contain no `try/catch` at all.

### Project structure

```
src/TaskTracker.Api/
├── Controllers/        AuthController, ProjectsController, TasksController
├── Services/            business logic (+ Interfaces/)
├── Repositories/         EF Core access (+ Interfaces/)
├── Models/
│   ├── Entities/        User, Project, ProjectMember, TaskItem
│   ├── Enums/            ProjectRole, TaskStatusEnum, TaskPriority
│   └── Dtos/             request/response DTOs
├── Data/                 ApplicationDbContext
├── Exceptions/           NotFoundException, ForbiddenException
├── Middleware/           GlobalExceptionHandler (IExceptionHandler)
└── Migrations/           EF Core migrations
```

## Data Model

- **User** can own multiple **Project**s (1-N, `OwnerId`)
- **Project** ⟷ **User**: many-to-many via the **ProjectMember** join table (`Role`: `Owner` / `Member`). When a project is created, its creator automatically also gets a `ProjectMember` row with `Owner` role.
- **Project** has many **TaskItem**s (1-N)
- **TaskItem** can optionally be assigned to a **User** (`AssignedToUserId`, nullable) and is always created by a **User** (`CreatedByUserId`, required — this field wasn't in the original requirements doc's data model, but was added later because the endpoint table specifies "task deletion is allowed for the creator or the project Owner")

All FK relationships pointing at `User` use `DeleteBehavior.Restrict` (to avoid SQL Server's "multiple cascade paths" error); `Project → ProjectMember` and `Project → TaskItem` relationships use `Cascade`.

## Setup

### Prerequisites

- .NET 9 SDK
- SQL Server (local, Developer Edition is fine)
- The `dotnet-ef` tool (already declared in the project manifest, installed in the step below)

### Steps

```bash
git clone <repo-url>
cd TaskTrackerApi

# Install the EF Core CLI tool
dotnet tool restore

cd src/TaskTracker.Api

# Restore packages
dotnet restore
```

**Connection string**: configured in `appsettings.json` as `Server=localhost;Database=TaskTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;`. Update it if you're using a different SQL Server instance.

**JWT signing key (important — not in the repo, must be set manually)**: for security reasons, `Jwt:Key` is stored in **.NET User Secrets** rather than `appsettings.json`, so anyone cloning the repo needs to generate their own key:

```bash
dotnet user-secrets set "Jwt:Key" "<a random string, at least 32 characters>"
```

If this step is skipped, the app fails fast on startup (intentionally — so you find out immediately, rather than only when someone tries to log in).

```bash
# Create the database / apply migrations
dotnet ef database update

# Run the app
dotnet run
```

Swagger UI: **`http://localhost:5133/swagger`** (the port may differ depending on `Properties/launchSettings.json`).

## Example Usage (via Swagger UI)

1. **`POST /api/auth/register`** to create a user → copy the `token` from the response.
2. Click the **Authorize** button at the top right of the page, paste the token (no `Bearer` prefix needed), Authorize → Close.
3. **`POST /api/projects`** to create a project (the creator automatically becomes `Owner`).
4. **`POST /api/projects/{id}/members`** to add another user to the project.
5. **`POST /api/projects/{projectId}/tasks`** to create a task.
6. **`GET /api/projects/{projectId}/tasks?status=Todo&priority=High&page=1&pageSize=20&sortBy=dueDate`** to try filtering/pagination.
7. **`PATCH /api/tasks/{id}/status`** and **`PATCH /api/tasks/{id}/assign`** to update status/assignment.
8. Register as a different user and try accessing a project you're not a member of → should get **403**. A request without a token → **401**.

## Endpoints

### Auth
| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |

### Projects
| Method | Route | Access |
|---|---|---|
| GET | `/api/projects` | Authenticated user (projects they belong to) |
| GET | `/api/projects/{id}` | Project member |
| POST | `/api/projects` | Authenticated user |
| PUT | `/api/projects/{id}` | Owner only |
| DELETE | `/api/projects/{id}` | Owner only |
| POST | `/api/projects/{id}/members` | Owner only |
| DELETE | `/api/projects/{id}/members/{userId}` | Owner only |

### Tasks
| Method | Route | Access |
|---|---|---|
| GET | `/api/projects/{projectId}/tasks` | Project member (filters: `status`, `priority`, `assignedUserId`; pagination: `page`, `pageSize`; sorting: `sortBy=dueDate\|priority\|createdAt`) |
| GET | `/api/tasks/{id}` | Project member |
| POST | `/api/projects/{projectId}/tasks` | Project member |
| PUT | `/api/tasks/{id}` | Project member |
| PATCH | `/api/tasks/{id}/status` | Project member |
| PATCH | `/api/tasks/{id}/assign` | Project member (can only assign to another project member) |
| DELETE | `/api/tasks/{id}` | The task's creator or the project's Owner |

## Error Format

All errors are returned as `{ "error": "..." }` (except validation errors, which use ASP.NET Core's standard `ValidationProblemDetails` format). Unexpected exceptions return 500 without leaking details to the client; the real details are only logged server-side.

## What I Learned Building This

This project was written from scratch with very little prior backend experience. Some highlights:

- **Why a layered architecture and DTOs matter**: never returning entities directly from the API (especially sensitive fields like `PasswordHash`), and keeping business logic out of controllers.
- **EF Core Code-First + Migrations**: learned by hitting it firsthand that SQL Server throws a "multiple cascade paths" error if `DeleteBehavior` isn't configured carefully when multiple foreign keys cascade-delete into the same table (`User`, in this project).
- **JWT authentication**: token generation (claims, signing key, expiration), validation via `TokenValidationParameters`, and why the signing key must never live in `appsettings.json` — it belongs in User Secrets.
- **Centralized error handling**: how to replace repetitive `try/catch` blocks in every controller with a single `IExceptionHandler` (ASP.NET Core 8+).
- **Real debugging experience** — actual bugs I hit and fixed while building this:
  - `Swashbuckle.AspNetCore 10.x` ships with the new `Microsoft.OpenApi 2.x`, which moved types out of the old `Microsoft.OpenApi.Models` namespace and replaced the `OpenApiReference` pattern — learned to read compiler errors and adapt to the new API shape (`OpenApiSecuritySchemeReference`, etc.) instead of relying on outdated tutorials.
  - `ControllerBase.Forbid(string)`'s parameter is an authentication *scheme name*, not a message — using it wrong returned 500 instead of 403; the fix is `StatusCode(403, ...)`.
  - `System.Text.Json` serializes enums as numbers by default — had to add a `JsonStringEnumConverter` to make the API usable with readable enum values like `"High"`.
  - When Swagger's "Authorize" button appeared to do nothing, the real problem wasn't in the UI but in `/swagger/v1/swagger.json` itself (a broken security reference, `"security": [{}]`) — inspecting the raw API output turned out to be a much faster way to debug than guessing from the UI.

## Out of Scope

Refresh tokens, email verification, file/attachment uploads, real-time notifications (SignalR), and a frontend are not part of this project's scope (see [task-tracker-gereksinimler.md](task-tracker-gereksinimler.md)).
