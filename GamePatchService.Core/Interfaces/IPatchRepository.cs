using GamePatchService.Core.Models;

namespace GamePatchService.Core.Interfaces;

public interface IPatchRepository
{
    Task<PatchFile?> GetByIdAsync(int id);
    Task<IEnumerable<PatchFile>> GetByGameIdAsync(int gameId);
    Task<PatchFile?> GetPatchPathAsync(int fromVersionId, int toVersionId);
    Task<PatchFile> AddAsync(PatchFile patchFile);
}
