namespace GamePatchService.Core.Models;

public class PatchDownloadInfo
{
    public int PatchId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public int DownloadId { get; set; }
}
