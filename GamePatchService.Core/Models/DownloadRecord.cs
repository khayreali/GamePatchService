namespace GamePatchService.Core.Models;

public class DownloadRecord
{
    public int Id { get; set; }
    public int PatchFileId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ClientIp { get; set; } = string.Empty;
    public DownloadStatus Status { get; set; }

    public PatchFile PatchFile { get; set; } = null!;
}
