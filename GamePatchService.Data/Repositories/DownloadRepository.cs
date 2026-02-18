using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data.Repositories;

public class DownloadRepository : IDownloadRepository
{
    private readonly GamePatchDbContext _db;

    public DownloadRepository(GamePatchDbContext dbContext)
    {
        _db = dbContext;
    }

    public async Task<DownloadRecord?> GetByIdAsync(int id)
    {
        return await _db.DownloadRecords
            .Include(d => d.PatchFile)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<DownloadRecord>> GetByPatchIdAsync(int patchFileId)
    {
        return await _db.DownloadRecords
            .Where(d => d.PatchFileId == patchFileId)
            .OrderByDescending(d => d.StartedAt)
            .ToListAsync();
    }

    public async Task<DownloadRecord> AddAsync(DownloadRecord record)
    {
        _db.DownloadRecords.Add(record);
        await _db.SaveChangesAsync();
        return record;
    }

    public async Task UpdateAsync(DownloadRecord record)
    {
        _db.Entry(record).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }
}
