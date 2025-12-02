using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamePatchService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameRepository _games;

    public GamesController(IGameRepository games)
    {
        _games = games;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Game>>> GetAll()
    {
        var games = await _games.GetAllAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> Get(int id)
    {
        var game = await _games.GetByIdWithVersionsAsync(id);
        if (game == null)
            return NotFound();
        return Ok(game);
    }

    [HttpPost]
    public async Task<ActionResult<Game>> Create(Game game)
    {
        if (string.IsNullOrWhiteSpace(game.Title))
            return BadRequest("Title is required");

        game.CreatedAt = DateTime.UtcNow;
        var created = await _games.AddAsync(game);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
}
