using GamePatchService.Core.Models;

namespace GamePatchService.Core.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(int id);
    Task<Game?> GetByIdWithVersionsAsync(int id);
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game> AddAsync(Game game);
}
