# Xlerion Leaderboard API

A production-ready REST API built with **ASP.NET Core 8** and **Entity Framework Core** for managing player scores, global rankings, and achievement systems across Xlerion games.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (PostgreSQL compatible) |
| Docs | Swagger / OpenAPI |

## Architecture

```
Controllers  →  ILeaderboardService  →  AppDbContext  →  SQL Server
   (HTTP)          (Business logic)        (EF Core)       (Data)
```

## Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/api/scores` | Submit a new game score |
| GET | `/api/leaderboard/{gameId}` | Top players (with optional region filter) |
| GET | `/api/players/{id}/stats` | Full stats for a player |
| POST | `/api/players` | Create a new player |
| POST | `/api/achievements/unlock` | Unlock an achievement |

## Quick Start

```bash
# 1. Clone and restore packages
git clone https://github.com/xlerion/leaderboard-api
cd XlerionLeaderboardAPI
dotnet restore

# 2. Configure connection string in appsettings.json
# (update Server and credentials as needed)

# 3. Apply migrations and seed data
dotnet ef database update

# 4. Run
dotnet run

# 5. Open Swagger UI
# http://localhost:5000
```

## Example Requests

**Submit a score:**
```json
POST /api/scores
{
  "playerId": 1,
  "gameId": "xlerion-arena",
  "value": 1500,
  "level": 12,
  "sessionDurationSeconds": 240.5,
  "metadata": "{\"difficulty\": \"hard\"}"
}
```

**Get LATAM leaderboard (top 5):**
```
GET /api/leaderboard/xlerion-arena?region=LATAM&top=5
```

**Unlock achievement:**
```json
POST /api/achievements/unlock
{
  "playerId": 1,
  "gameId": "xlerion-arena",
  "achievementCode": "CENTURION"
}
```

## Database Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Features

- Global and per-region leaderboards
- Best-score ranking (no duplicate players per leaderboard)
- Auto-unlock achievements on score submission
- Full player stats with recent history
- Seed data for local development
- CORS configured for `xlerion.com` and `localhost:3000`
- Swagger UI as default route in development

## Project Structure

```
XlerionLeaderboardAPI/
├── Controllers/       # HTTP layer — routing and serialization
├── Data/              # AppDbContext + EF configuration + SeedData
├── DTOs/              # Request/response shapes (records)
├── Models/            # Domain entities
├── Services/          # Business logic (ILeaderboardService)
├── Program.cs         # DI, middleware, startup
└── appsettings.json   # Configuration
```

---

Built by **[Xlerion](https://xlerion.com)** — Full Stack Developer + Game Designer, Bogotá.
