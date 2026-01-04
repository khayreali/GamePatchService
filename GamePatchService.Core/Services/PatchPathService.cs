using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;

namespace GamePatchService.Core.Services;

public class PatchPathService : IPatchPathService
{
    private readonly IVersionRepository _versions;
    private readonly IPatchRepository _patches;

    public PatchPathService(IVersionRepository versions, IPatchRepository patches)
    {
        _versions = versions;
        _patches = patches;
    }

    /*
     * Uses Dijkstra's algorithm to find the minimum-size patch path.
     * Versions are nodes, patches are directed edges, file sizes are weights.
     * Returns the sequence of patches that minimizes total download size.
     */
    public async Task<PatchPathResult> GetOptimalPatchPathAsync(int gameId, string fromVersion, string toVersion)
    {
        var sourceVersion = await _versions.GetByVersionNumberAsync(gameId, fromVersion);
        if (sourceVersion == null)
            return new PatchPathResult { Found = false, Error = "Source version not found" };

        var targetVersion = await _versions.GetByVersionNumberAsync(gameId, toVersion);
        if (targetVersion == null)
            return new PatchPathResult { Found = false, Error = "Target version not found" };

        if (sourceVersion.Id == targetVersion.Id)
            return new PatchPathResult { Found = true, TotalSizeBytes = 0 };

        var allPatches = (await _patches.GetByGameIdAsync(gameId)).ToList();
        if (allPatches.Count == 0)
            return new PatchPathResult { Found = false, Error = "No patches available" };

        var path = FindShortestPath(sourceVersion.Id, targetVersion.Id, allPatches);
        if (path == null)
            return new PatchPathResult { Found = false, Error = "No patch path exists" };

        var result = new PatchPathResult { Found = true };
        foreach (var patch in path)
        {
            result.Steps.Add(new PatchStep
            {
                PatchId = patch.Id,
                FromVersion = patch.FromVersion?.VersionNumber ?? "",
                ToVersion = patch.ToVersion?.VersionNumber ?? "",
                FileName = patch.FileName,
                SizeBytes = patch.SizeBytes
            });
            result.TotalSizeBytes += patch.SizeBytes;
        }

        return result;
    }

    private List<PatchFile>? FindShortestPath(int sourceId, int targetId, List<PatchFile> patches)
    {
        // Build adjacency list: fromVersionId -> list of patches
        var graph = new Dictionary<int, List<PatchFile>>();
        foreach (var patch in patches)
        {
            if (!graph.ContainsKey(patch.FromVersionId))
                graph[patch.FromVersionId] = new List<PatchFile>();
            graph[patch.FromVersionId].Add(patch);
        }

        // Distance from source to each node
        var dist = new Dictionary<int, long>();
        // Previous patch used to reach each node
        var prev = new Dictionary<int, PatchFile?>();
        // Priority queue: (distance, versionId)
        var pq = new SortedSet<(long dist, int id)>();

        dist[sourceId] = 0;
        prev[sourceId] = null;
        pq.Add((0, sourceId));

        while (pq.Count > 0)
        {
            var (currentDist, currentId) = pq.Min;
            pq.Remove(pq.Min);

            if (currentId == targetId)
                break;

            if (currentDist > dist.GetValueOrDefault(currentId, long.MaxValue))
                continue;

            if (!graph.ContainsKey(currentId))
                continue;

            foreach (var patch in graph[currentId])
            {
                var newDist = currentDist + patch.SizeBytes;
                var neighborId = patch.ToVersionId;

                if (newDist < dist.GetValueOrDefault(neighborId, long.MaxValue))
                {
                    // Remove old entry if exists
                    if (dist.ContainsKey(neighborId))
                        pq.Remove((dist[neighborId], neighborId));

                    dist[neighborId] = newDist;
                    prev[neighborId] = patch;
                    pq.Add((newDist, neighborId));
                }
            }
        }

        if (!prev.ContainsKey(targetId))
            return null;

        // Reconstruct path
        var path = new List<PatchFile>();
        int? current = targetId;
        while (current != null && prev.ContainsKey(current.Value) && prev[current.Value] != null)
        {
            var patch = prev[current.Value]!;
            path.Add(patch);
            current = patch.FromVersionId;
        }

        path.Reverse();
        return path;
    }
}
