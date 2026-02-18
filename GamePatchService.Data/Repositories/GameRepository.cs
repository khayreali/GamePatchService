using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data.Repositories;

public class GameRepository : IGameRepository
{
    private readonly GamePatchDbContext _context;

    public GameRepository(GamePatchDbContext db)
    {
        _context = db;
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games.FindAsync(id);
    }

    public async Task<Game?> GetByIdWithVersionsAsync(int id)
    {
        return await _context.Games
            .Include(g => g.Versions.OrderByDescending(v => v.ReleasedAt))
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games
            .OrderBy(g => g.Title)
            .ToListAsync();
    }

    public async Task<Game> AddAsync(Game game)
    {
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }
}
