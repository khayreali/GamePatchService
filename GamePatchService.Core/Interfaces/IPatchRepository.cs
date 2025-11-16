using GamePatchService.Core.Models;

namespace GamePatchService.Core.Interfaces;

public interface IPatchRepository
{
    Task<PatchFile?> GetByIdAsync(int id);
    Task<IEnumerable<PatchFile>> GetByGameIdAsync(int gameId);
    Task<PatchFile?> GetPatchPathAsync(int fromVersionId, int toVersionId);
    Task<IEnumerable<PatchFile>> GetPatchesFromVersionAsync(int versionId);
    Task<PatchFile> AddAsync(PatchFile patchFile);
    Task DeleteAsync(int id);
}
