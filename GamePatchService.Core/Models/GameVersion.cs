namespace GamePatchService.Core.Models;

public class GameVersion
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
    public long TotalSizeBytes { get; set; }
    public bool IsActive { get; set; }

    public Game Game { get; set; } = null!;
    public List<PatchFile> PatchesFrom { get; set; } = new();
    public List<PatchFile> PatchesTo { get; set; } = new();
}
