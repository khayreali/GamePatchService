# GamePatchService

Backend API that calculates the smallest download path to update a game between versions.

## The Problem

Say you want to update a game from v1.0 to v1.4. You could download one big 800 MB patch. But maybe downloading two smaller patches (v1.0 -> v1.2 -> v1.4) only adds up to 450 MB. This API figures out which way is smaller.

## Tech

- .NET 8 / ASP.NET Core
- PostgreSQL + Entity Framework Core
- Docker
- xUnit

## Setup

Start postgres:
```bash
docker compose up -d
```

Run it:
```bash
cd GamePatchService.API
dotnet run
```

Swagger docs at `http://localhost:5082/swagger`

## Endpoints

**Games**
- `GET /api/games` - list games
- `GET /api/games/{id}` - get game with versions
- `POST /api/games` - create game

**Versions**
- `GET /api/games/{gameId}/versions` - list versions
- `GET /api/games/{gameId}/versions/latest` - latest version
- `POST /api/games/{gameId}/versions` - create version

**Patches**
- `GET /api/patches/{fromVersionId}/{toVersionId}` - get patch between versions
- `GET /api/patches/{id}/download` - download patch
- `POST /api/patches` - create patch

**Optimal Path**
- `GET /api/games/{gameId}/patches/optimal?from=1.0.0&to=1.4.0` - calculates cheapest path

**Other**
- `GET /api/downloads/stats` - stats
- `GET /health` - health check

## Algorithm

Versions are nodes, patches are edges, edge weights are file sizes. Dijkstra's algorithm finds the minimum weight path. Pretty standard graph stuff.

## Structure

```
GamePatchService/
  GamePatchService.API/      # controllers, startup
  GamePatchService.Core/     # models, interfaces, path service
  GamePatchService.Data/     # ef core, repositories  
  GamePatchService.Tests/    # tests
```

## Tests

```bash
dotnet test
```

## Would be nice to add

- Auth on admin endpoints
- Actually serve patch files instead of just metadata
- Cache results
- Downgrade support
