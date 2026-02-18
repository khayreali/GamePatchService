using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamePatchService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadsController : ControllerBase
{
    private readonly IDownloadRepository _downloads;
    private readonly IPatchRepository _patches;
    private readonly IGameRepository _games;

    public DownloadsController(IDownloadRepository downloads, IPatchRepository patches, IGameRepository games)
    {
        _downloads = downloads;
        _patches = patches;
        _games = games;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DownloadStats>> GetStats()
    {
        var allGames = await _games.GetAllAsync();
        var stats = new DownloadStats();

        foreach (var game in allGames)
        {
            var patches = await _patches.GetByGameIdAsync(game.Id);
            var gameDownloads = 0;

            foreach (var patch in patches)
            {
                var downloads = await _downloads.GetByPatchIdAsync(patch.Id);
                var downloadList = downloads.ToList();
                gameDownloads += downloadList.Count;

                foreach (var d in downloadList)
                {
                    stats.TotalDownloads++;
                    stats.ByStatus.TryGetValue(d.Status, out var count);
                    stats.ByStatus[d.Status] = count + 1;
                }
            }

            if (gameDownloads > 0)
            {
                stats.ByGame[game.Title] = gameDownloads;
            }
        }

        return Ok(stats);
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> MarkComplete(int id)
    {
        var record = await _downloads.GetByIdAsync(id);
        if (record == null)
            return NotFound();

        record.Status = DownloadStatus.Completed;
        record.CompletedAt = DateTime.UtcNow;
        await _downloads.UpdateAsync(record);

        return NoContent();
    }

    [HttpPatch("{id}/fail")]
    public async Task<IActionResult> MarkFailed(int id)
    {
        var record = await _downloads.GetByIdAsync(id);
        if (record == null)
            return NotFound();

        record.Status = DownloadStatus.Failed;
        record.CompletedAt = DateTime.UtcNow;
        await _downloads.UpdateAsync(record);

        return NoContent();
    }
}
