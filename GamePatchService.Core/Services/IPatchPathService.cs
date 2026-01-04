using GamePatchService.Core.Models;

namespace GamePatchService.Core.Services;

public interface IPatchPathService
{
    Task<PatchPathResult> GetOptimalPatchPathAsync(int gameId, string fromVersion, string toVersion);
}

public class PatchPathResult
{
    public bool Found { get; set; }
    public string? Error { get; set; }
    public List<PatchStep> Steps { get; set; } = new();
    public long TotalSizeBytes { get; set; }
}

public class PatchStep
{
    public int PatchId { get; set; }
    public string FromVersion { get; set; } = string.Empty;
    public string ToVersion { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
