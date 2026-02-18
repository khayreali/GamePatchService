namespace GamePatchService.Core.Models;

public class DownloadStats
{
    public int TotalDownloads { get; set; }
    public Dictionary<string, int> ByGame { get; set; } = new();
    public Dictionary<DownloadStatus, int> ByStatus { get; set; } = new();
}
