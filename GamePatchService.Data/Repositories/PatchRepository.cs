using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data.Repositories;

public class PatchRepository : IPatchRepository
{
    private readonly GamePatchDbContext _db;

    public PatchRepository(GamePatchDbContext db)
    {
        _db = db;
    }

    public async Task<PatchFile?> GetByIdAsync(int id)
    {
        return await _db.PatchFiles
            .Include(p => p.FromVersion)
            .Include(p => p.ToVersion)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PatchFile>> GetByGameIdAsync(int gameId)
    {
        return await _db.PatchFiles
            .Include(p => p.FromVersion)
            .Include(p => p.ToVersion)
            .Where(p => p.FromVersion.GameId == gameId)
            .ToListAsync();
    }

    public async Task<PatchFile?> GetPatchPathAsync(int fromVersionId, int toVersionId)
    {
        return await _db.PatchFiles
            .Include(p => p.FromVersion)
            .Include(p => p.ToVersion)
            .FirstOrDefaultAsync(p => p.FromVersionId == fromVersionId && p.ToVersionId == toVersionId);
    }

    public async Task<PatchFile> AddAsync(PatchFile patchFile)
    {
        _db.PatchFiles.Add(patchFile);
        await _db.SaveChangesAsync();
        return patchFile;
    }
}
