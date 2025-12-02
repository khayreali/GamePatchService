using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamePatchService.API.Controllers;

[ApiController]
[Route("api/games/{gameId}/versions")]
public class VersionsController : ControllerBase
{
    private readonly IVersionRepository _versions;
    private readonly IGameRepository _games;

    public VersionsController(IVersionRepository versions, IGameRepository games)
    {
        _versions = versions;
        _games = games;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameVersion>>> GetByGame(int gameId)
    {
        var game = await _games.GetByIdAsync(gameId);
        if (game == null)
            return NotFound("Game not found");

        var versions = await _versions.GetByGameIdAsync(gameId);
        return Ok(versions);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<GameVersion>> GetLatest(int gameId)
    {
        var game = await _games.GetByIdAsync(gameId);
        if (game == null)
            return NotFound("Game not found");

        var version = await _versions.GetLatestVersionAsync(gameId);
        if (version == null)
            return NotFound("No active version found");

        return Ok(version);
    }

    [HttpPost]
    public async Task<ActionResult<GameVersion>> Create(int gameId, GameVersion version)
    {
        var game = await _games.GetByIdAsync(gameId);
        if (game == null)
            return NotFound("Game not found");

        if (string.IsNullOrWhiteSpace(version.VersionNumber))
            return BadRequest("VersionNumber is required");

        var existing = await _versions.GetByVersionNumberAsync(gameId, version.VersionNumber);
        if (existing != null)
            return BadRequest("Version already exists");

        version.GameId = gameId;
        version.ReleasedAt = DateTime.UtcNow;
        var created = await _versions.AddAsync(version);

        return CreatedAtAction(nameof(GetByGame), new { gameId }, created);
    }
}
