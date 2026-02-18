using GamePatchService.Core.Models;

namespace GamePatchService.Core.Interfaces;

public interface IVersionRepository
{
    Task<GameVersion?> GetByIdAsync(int id);
    Task<IEnumerable<GameVersion>> GetByGameIdAsync(int gameId);
    Task<GameVersion?> GetLatestVersionAsync(int gameId);
    Task<GameVersion?> GetByVersionNumberAsync(int gameId, string versionNumber);
    Task<GameVersion> AddAsync(GameVersion version);
}
