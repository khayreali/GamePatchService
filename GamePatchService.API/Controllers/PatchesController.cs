using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GamePatchService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatchesController : ControllerBase
{
    private readonly IPatchRepository _patches;
    private readonly IVersionRepository _versions;
    private readonly IDownloadRepository _downloads;

    public PatchesController(IPatchRepository patches, IVersionRepository versions, IDownloadRepository downloads)
    {
        _patches = patches;
        _versions = versions;
        _downloads = downloads;
    }

    [HttpGet("{fromVersionId}/{toVersionId}")]
    public async Task<ActionResult<PatchFile>> GetPatch(int fromVersionId, int toVersionId)
    {
        var fromVersion = await _versions.GetByIdAsync(fromVersionId);
        if (fromVersion == null)
            return NotFound("From version not found");

        var toVersion = await _versions.GetByIdAsync(toVersionId);
        if (toVersion == null)
            return NotFound("To version not found");

        var patch = await _patches.GetPatchPathAsync(fromVersionId, toVersionId);
        if (patch == null)
            return NotFound("Patch not found");

        return Ok(patch);
    }

    [HttpGet("{id}/download")]
    public async Task<ActionResult<PatchDownloadInfo>> Download(int id)
    {
        var patch = await _patches.GetByIdAsync(id);
        if (patch == null)
            return NotFound();

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var downloadRecord = new DownloadRecord
        {
            PatchFileId = id,
            StartedAt = DateTime.UtcNow,
            ClientIp = clientIp,
            Status = DownloadStatus.InProgress
        };
        await _downloads.AddAsync(downloadRecord);

        return Ok(new PatchDownloadInfo
        {
            PatchId = patch.Id,
            FileName = patch.FileName,
            SizeBytes = patch.SizeBytes,
            Checksum = patch.Checksum,
            DownloadId = downloadRecord.Id
        });
    }

    [HttpPost]
    public async Task<ActionResult<PatchFile>> Create(PatchFile patch)
    {
        var fromVersion = await _versions.GetByIdAsync(patch.FromVersionId);
        if (fromVersion == null)
            return BadRequest("From version not found");

        var toVersion = await _versions.GetByIdAsync(patch.ToVersionId);
        if (toVersion == null)
            return BadRequest("To version not found");

        if (fromVersion.GameId != toVersion.GameId)
            return BadRequest("Versions must belong to the same game");

        var existing = await _patches.GetPatchPathAsync(patch.FromVersionId, patch.ToVersionId);
        if (existing != null)
            return BadRequest("Patch already exists");

        patch.CreatedAt = DateTime.UtcNow;
        var created = await _patches.AddAsync(patch);

        return CreatedAtAction(nameof(GetPatch),
            new { fromVersionId = created.FromVersionId, toVersionId = created.ToVersionId },
            created);
    }
}
