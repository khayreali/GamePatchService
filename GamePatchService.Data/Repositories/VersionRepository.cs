using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data.Repositories;

public class VersionRepository : IVersionRepository
{
    private readonly GamePatchDbContext _db;

    public VersionRepository(GamePatchDbContext db)
    {
        _db = db;
    }

    public async Task<GameVersion?> GetByIdAsync(int id)
    {
        return await _db.GameVersions
            .Include(v => v.Game)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<GameVersion>> GetByGameIdAsync(int gameId)
    {
        return await _db.GameVersions
            .Where(v => v.GameId == gameId)
            .OrderByDescending(v => v.ReleasedAt)
            .ToListAsync();
    }

    public async Task<GameVersion?> GetLatestVersionAsync(int gameId)
    {
        return await _db.GameVersions
            .Where(v => v.GameId == gameId && v.IsActive)
            .OrderByDescending(v => v.ReleasedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<GameVersion?> GetByVersionNumberAsync(int gameId, string versionNumber)
    {
        return await _db.GameVersions
            .FirstOrDefaultAsync(v => v.GameId == gameId && v.VersionNumber == versionNumber);
    }

    public async Task<GameVersion> AddAsync(GameVersion version)
    {
        _db.GameVersions.Add(version);
        await _db.SaveChangesAsync();
        return version;
    }
}
