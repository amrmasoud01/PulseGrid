# PulseGrid

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-0ea5e9?style=for-the-badge)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![CQRS](https://img.shields.io/badge/Pattern-CQRS%20%2B%20MediatR-f59e0b?style=for-the-badge)](https://github.com/jbogard/MediatR)
[![SignalR](https://img.shields.io/badge/Real--Time-SignalR%20Core-ec4899?style=for-the-badge)](https://learn.microsoft.com/aspnet/core/signalr/)
[![Docker](https://img.shields.io/badge/Container-Docker%20Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-10b981?style=for-the-badge)](LICENSE)

PulseGrid is an enterprise-grade, distributed real-time uptime monitoring and automated incident mitigation platform built on **.NET 10**, **Clean Architecture**, and **SignalR Core**. It executes sub-second telemetry collection, evaluates health state transitions via an autonomous state machine, and broadcasts live infrastructure vitals to an interactive Blazor dashboard with zero page reloads.

---

## Architectural Blueprint & Directory Layout

PulseGrid adheres to strict Clean Architecture design principles with full separation of concerns, decoupling domain invariants from persistence, transport, and runtime mechanics.

```text
                               +----------------------------------+
                               |        Presentation Layer        |
                               |  +----------------------------+  |
                               |  |       PulseGrid.Web        |  |  (Blazor Server Interactive UI)
                               |  +----------------------------+  |
                               |  +----------------------------+  |
                               |  |       PulseGrid.Api        |  |  (Minimal APIs & SignalR StatusHub)
                               |  +----------------------------+  |
                               +-----------------+----------------+
                                                 |
                                                 v
                               +----------------------------------+
                               |     PulseGrid.Infrastructure     |
                               |  +----------------------------+  |
                               |  | EF Core 10 / SQL Server    |  |
                               |  | Background Monitoring Host |  |  (PeriodicTimer Engine)
                               |  +----------------------------+  |
                               +-----------------+----------------+
                                                 |
                                                 v
                               +----------------------------------+
                               |      PulseGrid.Application       |
                               |  +----------------------------+  |
                               |  | CQRS Commands & Queries    |  |
                               |  | MediatR Pipeline Behaviors |  |  (FluentValidation)
                               |  | DTOs & Service Contracts   |  |
                               |  +----------------------------+  |
                               +-----------------+----------------+
                                                 |
                                                 v
                               +----------------------------------+
                               |         PulseGrid.Domain         |
                               |  +----------------------------+  |
                               |  | Pure Entities & Enums      |  |  (Zero dependencies)
                               |  | MonitoredService, Incident |  |
                               |  +----------------------------+  |
                               +----------------------------------+
```

### Directory Structure

```text
PulseGrid/
├── docker-compose.yml                      # Root orchestration file for SQL Server, API, and Web
├── .dockerignore                           # Build artifact exclusion rules
├── README.md                               # System documentation
├── PulseGrid.slnx                          # Modern .NET solution definition
├── src/
│   ├── Core/
│   │   ├── PulseGrid.Domain/               # Pure business domain entities, enums, zero 3rd-party dependencies
│   │   │   ├── Entities/                   # MonitoredService, Incident, IncidentUpdate, HealthCheckLog
│   │   │   └── Enums/                      # ServiceStatus, IncidentSeverity, IncidentStatus
│   │   └── PulseGrid.Application/          # CQRS orchestrations, commands, queries, pipeline behaviors
│   │       ├── Common/                     # Behaviors (ValidationBehavior), Models (DTOs), Interfaces
│   │       ├── Incidents/                  # Incident domain commands & handlers
│   │       ├── Services/                   # Service registration, deletion, and toggle commands
│   │       └── SystemStatus/               # GetSystemStatus query and aggregate status evaluator
│   ├── Infrastructure/
│   │   ├── PulseGrid.Infrastructure/       # EF Core 10, SQL Server configuration, migrations
│   │   │   ├── Migrations/                 # Entity Framework migration snapshots
│   │   │   └── Persistence/                # ApplicationDbContext, Fluent API configurations
│   │   └── PulseGrid.BackgroundJobs/       # Autonomous background health-check engine
│   │       └── ServiceMonitoringWorker.cs  # Multi-threaded periodic ping worker (PeriodicTimer)
│   └── Presentation/
│       ├── PulseGrid.Api/                  # ASP.NET Core Minimal APIs, SignalR Hub, DI Composition
│       │   ├── Dockerfile                  # Multi-stage container definition
│       │   ├── Hubs/                       # StatusHub implementing IStatusHubClient
│       │   └── Services/                   # StatusNotifier bridge dispatching SignalR broadcasts
│       └── PulseGrid.Web/                  # Interactive Blazor Server real-time dashboard
│           ├── Dockerfile                  # Multi-stage container definition
│           └── Components/
│               ├── Layout/                 # MainLayout, NavMenu
│               └── Pages/                  # Home.razor (Live status cards, service controls, modals)
```

---

## Core Features

### 1. Asynchronous CQRS with MediatR Pipeline
Commands and queries are completely decoupled. Write commands (`CreateMonitoredServiceCommand`, `DeleteMonitoredServiceCommand`, `ToggleServiceMonitoringCommand`, `CreateIncidentCommand`) mutate state and enforce transactional consistency, while queries (`GetSystemStatusQuery`) execute fast, projection-based reads. Pipeline requests pass through a generic `ValidationBehavior<TRequest, TResponse>` validating domain inputs with FluentValidation before reaching domain handlers.

### 2. High-Throughput Telemetry Engine (`PeriodicTimer`)
`ServiceMonitoringWorker` runs asynchronously as a background hosted service within `PulseGrid.Api`. Leveraging .NET's `PeriodicTimer`, `IHttpClientFactory`, and `IServiceScopeFactory`, it concurrently polls active targets with discrete timeout enforcement, logs latency metrics in `HealthCheckLogs`, and handles connection drops without blocking worker threads.

### 3. Autonomous Incident State Machine
Monitored services automatically transition through defined operational states:
```text
[ Healthy / 200 OK ]  ──>  Operational (Latency tracked)
         │
         ├── Failures detected (Status != 200, Timeout, or Network Exception)
         v
[ Degraded / Down ]   ──>  Auto-generate Incident with Severity (Minor / Major / Critical)
         │
         ├── Endpoint recovers (Success verified)
         v
[ Auto-Resolution ]   ──>  Mark Incident Resolved, timestamp snapshot, return to Operational
```

### 4. Full-Duplex SignalR Core Telemetry Broadcasts
Live metrics propagate via `IStatusHubClient` across two real-time channels:
- `ReceiveServicePing(Guid serviceId, ServiceStatus status, long latencyMs)`: Instant latency pulse and card glow on the dashboard.
- `ReceiveStatusUpdate(SystemStatusOverviewDto overview)`: System-wide status re-evaluation and incident feed updates.

### 5. Management & Cascade Operations
- **Dynamic Pause/Resume Toggle**: Instantly pause or resume monitoring for individual services without removing audit history.
- **Cascade Deletion**: Safely purges monitored endpoints and all associated health check records and incident timelines.

---

## Minimal API Reference

`PulseGrid.Api` exposes high-performance Minimal API endpoints:

| HTTP Method | Route | Request Body | Status Codes | Description |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/status` | _None_ | `200 OK` | Retrieves the aggregated system overview, active services, and open incidents. |
| `POST` | `/api/services` | JSON (`Name`, `TargetUrl`, `IntervalSeconds`, `TimeoutMs`) | `201 Created`, `400 Bad Request` | Registers a new service for health monitoring and initiates polling. |
| `DELETE` | `/api/services/{id:guid}` | _None_ | `204 NoContent`, `404 Not Found` | Deletes a service and cascade-removes all logs and incident history. |
| `PUT` | `/api/services/{id:guid}/toggle` | _None_ | `200 OK` (`{ isActive: bool }`), `404 Not Found` | Toggles the active monitoring state between paused and running. |

SignalR Hub endpoint:
- `WS /hubs/status` — StatusHub WebSocket for real-time telemetry streaming.

OpenAPI Specification:
- `GET /openapi/v1.json` — Machine-readable OpenAPI 3.0 document.

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Engine & Docker Compose](https://docs.docker.com/get-docker/)
- Local SQL Server 2022 (only if running without Docker)

---

### Option 1: Docker Deployment (Recommended)

To orchestrate the complete stack (SQL Server 2022, API, and Web Dashboard) in a single step:

```bash
# Clone the repository
git clone https://github.com/amrmasoud01/PulseGrid.git
cd PulseGrid

# Build and start all services in detached mode
docker compose up --build -d
```

#### Service URLs
- **Web Dashboard**: [http://localhost:5000](http://localhost:5000)
- **REST API & SignalR**: [http://localhost:5103](http://localhost:5103)
- **OpenAPI Document**: [http://localhost:5103/openapi/v1.json](http://localhost:5103/openapi/v1.json)
- **SQL Server**: `localhost:1433` (sa / `YourStrong@Password123`)

To stop the containers and clean up:
```bash
docker compose down
```

---

### Option 2: Local Development Setup

1. **Start SQL Server** (via Docker or local instance):
   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. **Restore and Build the Solution**:
   ```bash
   dotnet restore PulseGrid.slnx
   dotnet build PulseGrid.slnx
   ```

3. **Run `PulseGrid.Api`** (Database migrations apply automatically on startup):
   ```bash
   dotnet run --project src/Presentation/PulseGrid.Api
   ```

4. **Run `PulseGrid.Web`** (in a separate terminal):
   ```bash
   dotnet run --project src/Presentation/PulseGrid.Web
   ```

5. Navigate to [http://localhost:5163](http://localhost:5163) (or configured port) to interact with the dashboard.

---

## Author & Credits

Designed and developed by:
**Amr Masoud** — Lead Architect & Backend Engineer
- GitHub: [@amrmasoud01](https://github.com/amrmasoud01)
- Repository: [PulseGrid](https://github.com/amrmasoud01/PulseGrid)
