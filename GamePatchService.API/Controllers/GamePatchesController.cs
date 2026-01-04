using GamePatchService.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamePatchService.API.Controllers;

[ApiController]
[Route("api/games/{gameId}/patches")]
public class GamePatchesController : ControllerBase
{
    private readonly IPatchPathService _pathService;

    public GamePatchesController(IPatchPathService pathService)
    {
        _pathService = pathService;
    }

    [HttpGet("optimal")]
    public async Task<ActionResult<PatchPathResult>> GetOptimalPath(
        int gameId,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (string.IsNullOrWhiteSpace(from))
            return BadRequest("'from' version is required");

        if (string.IsNullOrWhiteSpace(to))
            return BadRequest("'to' version is required");

        var result = await _pathService.GetOptimalPatchPathAsync(gameId, from, to);

        if (!result.Found)
            return NotFound(result.Error);

        return Ok(result);
    }
}
