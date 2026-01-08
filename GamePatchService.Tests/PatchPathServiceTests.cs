using GamePatchService.Core.Interfaces;
using GamePatchService.Core.Models;
using GamePatchService.Core.Services;
using Moq;

namespace GamePatchService.Tests;

public class PatchPathServiceTests
{
    private readonly Mock<IVersionRepository> _versionRepo;
    private readonly Mock<IPatchRepository> _patchRepo;
    private readonly PatchPathService _service;

    public PatchPathServiceTests()
    {
        _versionRepo = new Mock<IVersionRepository>();
        _patchRepo = new Mock<IPatchRepository>();
        _service = new PatchPathService(_versionRepo.Object, _patchRepo.Object);
    }

    [Fact]
    public async Task SameVersion_ReturnsEmptyPath()
    {
        var version = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(version);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "1.0.0");

        Assert.True(result.Found);
        Assert.Empty(result.Steps);
        Assert.Equal(0, result.TotalSizeBytes);
    }

    [Fact]
    public async Task SourceVersionNotFound_ReturnsError()
    {
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync((GameVersion?)null);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "2.0.0");

        Assert.False(result.Found);
        Assert.Equal("Source version not found", result.Error);
    }

    [Fact]
    public async Task TargetVersionNotFound_ReturnsError()
    {
        var source = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(source);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "2.0.0")).ReturnsAsync((GameVersion?)null);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "2.0.0");

        Assert.False(result.Found);
        Assert.Equal("Target version not found", result.Error);
    }

    [Fact]
    public async Task NoPatches_ReturnsError()
    {
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "2.0.0" };
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "2.0.0")).ReturnsAsync(v2);
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(new List<PatchFile>());

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "2.0.0");

        Assert.False(result.Found);
        Assert.Equal("No patches available", result.Error);
    }

    [Fact]
    public async Task NoPathExists_ReturnsError()
    {
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "1.1.0" };
        var v3 = new GameVersion { Id = 3, GameId = 1, VersionNumber = "2.0.0" };
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "2.0.0")).ReturnsAsync(v3);

        // Only patch from 1.0 -> 1.1, no path to 2.0
        var patches = new List<PatchFile>
        {
            new() { Id = 1, FromVersionId = 1, ToVersionId = 2, FromVersion = v1, ToVersion = v2, SizeBytes = 100 }
        };
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(patches);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "2.0.0");

        Assert.False(result.Found);
        Assert.Equal("No patch path exists", result.Error);
    }

    [Fact]
    public async Task DirectPatch_ReturnsSingleStep()
    {
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "2.0.0" };
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "2.0.0")).ReturnsAsync(v2);

        var patches = new List<PatchFile>
        {
            new() { Id = 1, FromVersionId = 1, ToVersionId = 2, FromVersion = v1, ToVersion = v2, FileName = "patch-1-2.bin", SizeBytes = 500 }
        };
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(patches);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "2.0.0");

        Assert.True(result.Found);
        Assert.Single(result.Steps);
        Assert.Equal(500, result.TotalSizeBytes);
        Assert.Equal("1.0.0", result.Steps[0].FromVersion);
        Assert.Equal("2.0.0", result.Steps[0].ToVersion);
    }

    [Fact]
    public async Task LinearPath_ReturnsAllSteps()
    {
        // 1.0 -> 1.1 -> 1.2 -> 1.3
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "1.1.0" };
        var v3 = new GameVersion { Id = 3, GameId = 1, VersionNumber = "1.2.0" };
        var v4 = new GameVersion { Id = 4, GameId = 1, VersionNumber = "1.3.0" };

        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.3.0")).ReturnsAsync(v4);

        var patches = new List<PatchFile>
        {
            new() { Id = 1, FromVersionId = 1, ToVersionId = 2, FromVersion = v1, ToVersion = v2, SizeBytes = 100 },
            new() { Id = 2, FromVersionId = 2, ToVersionId = 3, FromVersion = v2, ToVersion = v3, SizeBytes = 100 },
            new() { Id = 3, FromVersionId = 3, ToVersionId = 4, FromVersion = v3, ToVersion = v4, SizeBytes = 100 }
        };
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(patches);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "1.3.0");

        Assert.True(result.Found);
        Assert.Equal(3, result.Steps.Count);
        Assert.Equal(300, result.TotalSizeBytes);
        Assert.Equal("1.0.0", result.Steps[0].FromVersion);
        Assert.Equal("1.1.0", result.Steps[1].FromVersion);
        Assert.Equal("1.2.0", result.Steps[2].FromVersion);
    }

    [Fact]
    public async Task MultiHopSmallerThanDirect_ChoosesMultiHop()
    {
        // Direct: 1.0 -> 1.3 = 1000 bytes
        // Multi:  1.0 -> 1.1 -> 1.2 -> 1.3 = 100 + 100 + 100 = 300 bytes
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "1.1.0" };
        var v3 = new GameVersion { Id = 3, GameId = 1, VersionNumber = "1.2.0" };
        var v4 = new GameVersion { Id = 4, GameId = 1, VersionNumber = "1.3.0" };

        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.3.0")).ReturnsAsync(v4);

        var patches = new List<PatchFile>
        {
            new() { Id = 1, FromVersionId = 1, ToVersionId = 4, FromVersion = v1, ToVersion = v4, SizeBytes = 1000 },
            new() { Id = 2, FromVersionId = 1, ToVersionId = 2, FromVersion = v1, ToVersion = v2, SizeBytes = 100 },
            new() { Id = 3, FromVersionId = 2, ToVersionId = 3, FromVersion = v2, ToVersion = v3, SizeBytes = 100 },
            new() { Id = 4, FromVersionId = 3, ToVersionId = 4, FromVersion = v3, ToVersion = v4, SizeBytes = 100 }
        };
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(patches);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "1.3.0");

        Assert.True(result.Found);
        Assert.Equal(3, result.Steps.Count);
        Assert.Equal(300, result.TotalSizeBytes);
    }

    [Fact]
    public async Task DirectSmallerThanMultiHop_ChoosesDirect()
    {
        // Direct: 1.0 -> 1.3 = 200 bytes
        // Multi:  1.0 -> 1.1 -> 1.2 -> 1.3 = 100 + 100 + 100 = 300 bytes
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "1.1.0" };
        var v3 = new GameVersion { Id = 3, GameId = 1, VersionNumber = "1.2.0" };
        var v4 = new GameVersion { Id = 4, GameId = 1, VersionNumber = "1.3.0" };

        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.3.0")).ReturnsAsync(v4);

        var patches = new List<PatchFile>
        {
            new() { Id = 1, FromVersionId = 1, ToVersionId = 4, FromVersion = v1, ToVersion = v4, SizeBytes = 200 },
            new() { Id = 2, FromVersionId = 1, ToVersionId = 2, FromVersion = v1, ToVersion = v2, SizeBytes = 100 },
            new() { Id = 3, FromVersionId = 2, ToVersionId = 3, FromVersion = v2, ToVersion = v3, SizeBytes = 100 },
            new() { Id = 4, FromVersionId = 3, ToVersionId = 4, FromVersion = v3, ToVersion = v4, SizeBytes = 100 }
        };
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(patches);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "1.3.0");

        Assert.True(result.Found);
        Assert.Single(result.Steps);
        Assert.Equal(200, result.TotalSizeBytes);
    }

    [Fact]
    public async Task ComplexGraph_FindsOptimalPath()
    {
        // Graph:
        //        100      200
        //   1.0 -----> 1.1 -----> 1.3
        //    |                    ^
        //    |         50         |
        //    +-------> 1.2 -------+
        //       150         80
        //
        // Best path: 1.0 -> 1.2 -> 1.3 = 150 + 80 = 230
        // Alt path:  1.0 -> 1.1 -> 1.3 = 100 + 200 = 300
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "1.1.0" };
        var v3 = new GameVersion { Id = 3, GameId = 1, VersionNumber = "1.2.0" };
        var v4 = new GameVersion { Id = 4, GameId = 1, VersionNumber = "1.3.0" };

        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.3.0")).ReturnsAsync(v4);

        var patches = new List<PatchFile>
        {
            new() { Id = 1, FromVersionId = 1, ToVersionId = 2, FromVersion = v1, ToVersion = v2, SizeBytes = 100 },
            new() { Id = 2, FromVersionId = 2, ToVersionId = 4, FromVersion = v2, ToVersion = v4, SizeBytes = 200 },
            new() { Id = 3, FromVersionId = 1, ToVersionId = 3, FromVersion = v1, ToVersion = v3, SizeBytes = 150 },
            new() { Id = 4, FromVersionId = 3, ToVersionId = 4, FromVersion = v3, ToVersion = v4, SizeBytes = 80 }
        };
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(patches);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "1.3.0");

        Assert.True(result.Found);
        Assert.Equal(2, result.Steps.Count);
        Assert.Equal(230, result.TotalSizeBytes);
        Assert.Equal("1.0.0", result.Steps[0].FromVersion);
        Assert.Equal("1.2.0", result.Steps[0].ToVersion);
        Assert.Equal("1.2.0", result.Steps[1].FromVersion);
        Assert.Equal("1.3.0", result.Steps[1].ToVersion);
    }

    [Fact]
    public async Task DiamondGraph_FindsOptimalPath()
    {
        //       1.1
        //      / 50 \ 50
        //   1.0       1.3
        //      \ 30 / 40
        //       1.2
        //
        // Top path: 50 + 50 = 100
        // Bottom path: 30 + 40 = 70 (optimal)
        var v1 = new GameVersion { Id = 1, GameId = 1, VersionNumber = "1.0.0" };
        var v2 = new GameVersion { Id = 2, GameId = 1, VersionNumber = "1.1.0" };
        var v3 = new GameVersion { Id = 3, GameId = 1, VersionNumber = "1.2.0" };
        var v4 = new GameVersion { Id = 4, GameId = 1, VersionNumber = "1.3.0" };

        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.0.0")).ReturnsAsync(v1);
        _versionRepo.Setup(r => r.GetByVersionNumberAsync(1, "1.3.0")).ReturnsAsync(v4);

        var patches = new List<PatchFile>
        {
            new() { Id = 1, FromVersionId = 1, ToVersionId = 2, FromVersion = v1, ToVersion = v2, SizeBytes = 50 },
            new() { Id = 2, FromVersionId = 2, ToVersionId = 4, FromVersion = v2, ToVersion = v4, SizeBytes = 50 },
            new() { Id = 3, FromVersionId = 1, ToVersionId = 3, FromVersion = v1, ToVersion = v3, SizeBytes = 30 },
            new() { Id = 4, FromVersionId = 3, ToVersionId = 4, FromVersion = v3, ToVersion = v4, SizeBytes = 40 }
        };
        _patchRepo.Setup(r => r.GetByGameIdAsync(1)).ReturnsAsync(patches);

        var result = await _service.GetOptimalPatchPathAsync(1, "1.0.0", "1.3.0");

        Assert.True(result.Found);
        Assert.Equal(2, result.Steps.Count);
        Assert.Equal(70, result.TotalSizeBytes);
        Assert.Equal("1.2.0", result.Steps[0].ToVersion);
    }
}
