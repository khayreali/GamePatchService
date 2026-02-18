namespace GamePatchService.Core.Models;

public class PatchFile
{
    public int Id { get; set; }
    public int FromVersionId { get; set; }
    public int ToVersionId { get; set; }
    public string FileName { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Checksum { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public GameVersion FromVersion { get; set; } = null!;
    public GameVersion ToVersion { get; set; } = null!;
    public List<DownloadRecord> Downloads { get; set; } = new();
}
