using GamePatchService.Core.Models;

namespace GamePatchService.Tests;

public class ModelTests
{
    [Fact]
    public void Game_Versions_InitializesEmpty()
    {
        var game = new Game();
        Assert.NotNull(game.Versions);
        Assert.Empty(game.Versions);
    }

    [Fact]
    public void GameVersion_DefaultIsActive_IsFalse()
    {
        var version = new GameVersion();
        Assert.False(version.IsActive);
    }

    [Fact]
    public void PatchFile_HasBothVersionNavigations()
    {
        var from = new GameVersion { Id = 1, VersionNumber = "1.0.0" };
        var to = new GameVersion { Id = 2, VersionNumber = "1.1.0" };
        var patch = new PatchFile
        {
            FromVersionId = from.Id,
            ToVersionId = to.Id,
            FromVersion = from,
            ToVersion = to
        };

        Assert.Equal(1, patch.FromVersionId);
        Assert.Equal(2, patch.ToVersionId);
    }

    [Fact]
    public void DownloadRecord_DefaultStatus_IsQueued()
    {
        var record = new DownloadRecord();
        Assert.Equal(DownloadStatus.Queued, record.Status);
    }

    [Fact]
    public void DownloadStatus_HasExpectedValues()
    {
        Assert.Equal(0, (int)DownloadStatus.Queued);
        Assert.Equal(1, (int)DownloadStatus.InProgress);
        Assert.Equal(2, (int)DownloadStatus.Completed);
        Assert.Equal(3, (int)DownloadStatus.Failed);
    }
}
