namespace GamePatchService.Core.Models;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<GameVersion> Versions { get; set; } = new();
}
