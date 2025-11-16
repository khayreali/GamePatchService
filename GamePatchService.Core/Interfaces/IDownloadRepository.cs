using GamePatchService.Core.Models;

namespace GamePatchService.Core.Interfaces;

public interface IDownloadRepository
{
    Task<DownloadRecord?> GetByIdAsync(int id);
    Task<IEnumerable<DownloadRecord>> GetByPatchIdAsync(int patchFileId);
    Task<IEnumerable<DownloadRecord>> GetByStatusAsync(DownloadStatus status);
    Task<IEnumerable<DownloadRecord>> GetRecentAsync(int count);
    Task<DownloadRecord> AddAsync(DownloadRecord record);
    Task UpdateAsync(DownloadRecord record);
    Task DeleteAsync(int id);
}
