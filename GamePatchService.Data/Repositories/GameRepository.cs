using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data.Repositories;

public class GameRepository : IGameRepository
{
    private readonly GamePatchDbContext _db;

    public GameRepository(GamePatchDbContext db)
    {
        _db = db;
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _db.Games.FindAsync(id);
    }

    public async Task<Game?> GetByIdWithVersionsAsync(int id)
    {
        return await _db.Games
            .Include(g => g.Versions.OrderByDescending(v => v.ReleasedAt))
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _db.Games
            .OrderBy(g => g.Title)
            .ToListAsync();
    }

    public async Task<Game> AddAsync(Game game)
    {
        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        return game;
    }

    public async Task UpdateAsync(Game game)
    {
        _db.Entry(game).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game != null)
        {
            _db.Games.Remove(game);
            await _db.SaveChangesAsync();
        }
    }
}
