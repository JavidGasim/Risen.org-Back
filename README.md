# Risen.org Backend

Backend for Risen.org — a gamified engineering learning platform.

Tech: ASP.NET Core (.NET 8), EF Core, Identity, Serilog

Contents
- Overview
- Quick start
- Configuration
- Database & migrations
- Endpoints (summary)
- Core systems & behavior
- Admin & audit
- Background services
- Testing
- Operational notes

---

## Overview

This repository implements the backend API and core business systems for Risen.org:

- XP system (idempotent awards, transactions, rounding rules)
- Streak system (daily activity tracking and streak bonus)
- League system (tiers based on total XP and recorded history)
- Risen Score (performance formula)
- Quest system (learning tasks, per-day limits, premium/advanced gating)
- Leaderboards and university candidate filtering
- Admin auditing, revokes and archival of transactions

All ranking and calculations are done server-side.

## Quick start (development)

Requirements:
- .NET 8 SDK
- SQL Server for production; SQLite can be used for tests
- Visual Studio 2026 or VS Code

Run locally:

1. Restore and build:

   dotnet build

2. Configure connection string and JWT in `appsettings.json` (see Configuration).

3. Apply EF migrations to local database:

   dotnet ef database update --project Risen.DataAccess --startup-project Risen.Web

4. Run the API:

   dotnet run --project Risen.Web

5. Swagger available in Development environment at `/swagger`.

## Configuration

Key sections in appsettings.json:

- `ConnectionStrings:Default` — SQL Server connection.
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` — token settings.
- `QuestPolicy` — quest multipliers and streak bonus.
- `Retention` — retention/archival options for XpTransactions.

Example Retention config:

```json
"Retention": {
  "TransactionRetentionDays": 365,
  "IntervalMinutes": 60,
  "BatchSize": 1000
}
```

## Database & Migrations

EF migrations live in `Risen.DataAccess/Migrations`. Important schema aspects:

- `UserStats` includes `TotalXp`, `CurrentStreak`, `LastStreakDateUtc`, `RisenScore`.
- `LeagueTiers` include `MinXp`, `MaxXp`, `Weight`.
- `XpTransactions` stores all XP awards and has unique index on `(UserId, SourceType, SourceKey)` to enforce idempotency.
- `AdminActions` stores admin audit records.
- `XpTransactionArchives` stores archived transactions moved by the RetentionService.

Apply migrations before running against a DB.

## Endpoints (summary)

Authentication: JWT Bearer. Many endpoints require `Authorize` and some require `Admin` or `University` roles.

Main endpoints (paths are prefixed with `/api`):

- XpController
  - POST `/api/xp/award` (Admin) — Award XP (idempotent). Request: `AwardXpRequest`.
  - POST `/api/xp/revoke` (Admin) — Revoke (compensating negative transaction). Request: `RevokeXpRequest`.

- GamificationController
  - POST `/api/gamification/award-xp` (Admin) — alternative award route.

- QuestsController
  - POST `/api/quests/submit` (Auth) — Submit quest answer. Request: `SubmitQuestAnswerRequest`. Returns `SubmitQuestAnswerResponse` including awarded XP and updated streak/league.

- LeaderboardsController
  - GET `/api/leaderboards/global` — global leaderboard (query `league`, `limit`, `offset`).
  - GET `/api/leaderboards/university/{universityId}` — university leaderboard.
  - GET `/api/leaderboards/my-university` (Auth) — leaderboard for user's university.
  - GET `/api/leaderboards/my-rank` (Auth) — returns user's rank (optionally university-only).

- StatsController
  - GET `/api/stats/me` (Auth) — returns `MeStatsDto` with total XP, RisenScore, streaks and plan.

- LeaguesController
  - GET `/api/leagues/me` (Auth) — returns current user's league and total XP.

- UniversitiesController
  - GET `/api/universities/suggest?q=&limit=` — suggest university names.
  - GET `/api/universities/search?q=&country=&limit=&offset=` — search universities.
  - GET `/api/universities/candidates` (Admin/University) — candidate filter by min league/min score/country.

- AdminController (Admin only)
  - GET `/api/admin/actions` — list admin audit actions.
  - GET `/api/admin/xp-transactions` — list XpTransactions with filters: `userId`, `sourceType`, `adminId`, `since`, `limit`, `offset`.

Refer to `Risen.Contracts` for precise request/response DTO definitions.

## Core behavior & rules

- Idempotency: award calls must provide a stable `SourceKey` (e.g., `Quest:{id}:date:YYYYMMDD`) to ensure only one award per logical event. DB uniqueness enforces this and service handles DbUpdateException on races.
- XP calculation: `FinalXp = Round(BaseXp * multiplier)` with multiplier capped at 10 and final min 1 XP.
- Streaks: day boundaries use UTC.Date. If local day semantics are required, store per-user timezone and adjust logic.
- League transitions: when `TotalXp` changes, `FindTierByXpAsync` finds the new tier. A `UserLeagueHistory` row is inserted on changes.
- Admin revokes: create compensating negative transactions rather than deleting existing data to preserve the audit trail.

## Admin & audit

- Admin actions are recorded in `AdminActions` and admin adjustments record `AdminId` and `AdminReason` on `XpTransaction`.
- Admin API endpoints allow querying transactions and actions for audit.

## Background services

- `RetentionService`: periodically archives old `XpTransactions` into `XpTransactionArchives` in batches (atomic per batch).
- Logs archive counts and errors.
- Default retention: keep 365 days (configurable).

## Testing

- Tests are in `Risen.Tests`. Use `dotnet test` to run.
- Integration-style tests use SQLite in-memory for transactional behavior and EF InMemory for simpler cases.

## Operational notes

- Always protect award/revoke endpoints (Admin role) and restrict access to trusted personnel/services.
- Log admin actions and preserve audit trail (AdminAction and XpTransaction.Admin*).
- Add rate limiting and monitoring on awarding endpoints to detect abuse.
- Plan an archival/purge strategy for `XpTransactionArchives` if long-term retention is undesirable in DB.

## Where to look in the code

- Business rules: `Risen.Business/Services/*` (XpService, QuestService, LeaderboardService, StatsService).
- EF models and DbContext configuration: `Risen.DataAccess/Data/AppDbContext.cs`.
- DTOs / contracts: `Risen.Contracts/*`.
- Controllers / API surface: `Risen.Web/Controllers/*`.
- Background services: `Risen.Web/Services/RetentionService.cs`.

## Next steps & suggestions

- Add more unit and integration tests around concurrency, revoke flows and leaderboard ranking.
- Add OpenAPI examples and security notes for client developers.
- Expose RisenScore parameters via configuration to allow tuning without code changes.

---

For more details, read the code in the `Risen.Business` and `Risen.DataAccess` folders and consult the DTO definitions in `Risen.Contracts` for exact schemas.
