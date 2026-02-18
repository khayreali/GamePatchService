using GamePatchService.Core.Models;

namespace GamePatchService.Core.Interfaces;

public interface IDownloadRepository
{
    Task<DownloadRecord?> GetByIdAsync(int id);
    Task<IEnumerable<DownloadRecord>> GetByPatchIdAsync(int patchFileId);
    Task<DownloadRecord> AddAsync(DownloadRecord record);
    Task UpdateAsync(DownloadRecord record);
}
