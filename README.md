# GamePatchService

A REST API for managing game patches and calculating optimal download paths. Built as a demonstration of graph algorithms applied to a practical software distribution problem.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8 with PostgreSQL
- xUnit + Moq for testing
- Docker Compose for local development

## Running Locally

Start the PostgreSQL database:

```bash
docker-compose up -d
```

Run the API:

```bash
cd GamePatchService.API
dotnet run
```

The API will be available at `https://localhost:5001` (or check the console output for the actual port).

Swagger UI is available at `/swagger` in development mode.

## API Endpoints

### Games

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/games` | List all games |
| GET | `/api/games/{id}` | Get game with its versions |
| POST | `/api/games` | Create a new game |

### Versions

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/games/{gameId}/versions` | List versions for a game |
| GET | `/api/games/{gameId}/versions/latest` | Get the latest active version |
| POST | `/api/games/{gameId}/versions` | Create a new version |

### Patches

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/patches/{fromVersionId}/{toVersionId}` | Get patch info between two versions |
| GET | `/api/patches/{id}/download` | Get download info (creates download record) |
| POST | `/api/patches` | Create a new patch |

### Optimal Path

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/games/{gameId}/patches/optimal?from=1.0.0&to=1.4.0` | Calculate optimal patch path |

### Downloads

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/downloads/stats` | Download statistics |
| PATCH | `/api/downloads/{id}/complete` | Mark download as completed |
| PATCH | `/api/downloads/{id}/fail` | Mark download as failed |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check (includes DB connectivity) |

## Patch Path Optimization

The core feature is finding the minimum-size download path between two game versions. This matters because:

- A direct patch from v1.0 to v1.4 might be 800 MB (lots of changed files)
- But going v1.0 -> v1.2 -> v1.4 might only be 450 MB total (incremental changes)

The algorithm models this as a shortest-path problem:

- **Nodes**: Game versions
- **Edges**: Available patches (directed, from older to newer)
- **Weights**: Patch file sizes in bytes

We use Dijkstra's algorithm with a priority queue to find the path that minimizes total download size. The algorithm runs in O((V + E) log V) time where V is the number of versions and E is the number of patches.

### Example

For a game with patches structured like:

```
        1.1
       /150\ 200
    1.0     1.4
       \200/ 250
        1.2
```

The optimal path from 1.0 to 1.4 would be 1.0 -> 1.2 -> 1.4 (450 MB) rather than 1.0 -> 1.1 -> 1.4 (350 MB)... wait, actually in this case the top path wins. The algorithm handles all these cases correctly regardless of the graph structure.

## Project Structure

```
GamePatchService/
  GamePatchService.API/        # Controllers, Program.cs
  GamePatchService.Core/       # Models, interfaces, services
  GamePatchService.Data/       # EF Core DbContext, repositories
  GamePatchService.Tests/      # Unit tests
```

## Running Tests

```bash
dotnet test
```

## Future Improvements

- Add authentication/authorization for admin endpoints (creating games, versions, patches)
- Implement actual file storage and streaming for patch downloads (currently just returns metadata)
- Add caching for the optimal path calculation since the patch graph doesn't change frequently
- Support for rollback patches (going from newer to older versions)
- Rate limiting on the download endpoint
