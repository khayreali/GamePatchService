using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data;

public static class SeedData
{
    public static async Task InitializeAsync(GamePatchDbContext db)
    {
        if (await db.Games.AnyAsync())
            return;

        // Game 1: Space Explorer
        var spaceExplorer = new Game
        {
            Title = "Space Explorer",
            Publisher = "Stellar Games",
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        // Game 2: Dragon Quest Online
        var dragonQuest = new Game
        {
            Title = "Dragon Quest Online",
            Publisher = "Fantasy Studios",
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        // Game 3: Racing Thunder
        var racingThunder = new Game
        {
            Title = "Racing Thunder",
            Publisher = "Speed Interactive",
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        db.Games.AddRange(spaceExplorer, dragonQuest, racingThunder);
        await db.SaveChangesAsync();

        // Space Explorer versions
        var se100 = new GameVersion { GameId = spaceExplorer.Id, VersionNumber = "1.0.0", ReleasedAt = DateTime.UtcNow.AddMonths(-6), TotalSizeBytes = 5_000_000_000, IsActive = true };
        var se110 = new GameVersion { GameId = spaceExplorer.Id, VersionNumber = "1.1.0", ReleasedAt = DateTime.UtcNow.AddMonths(-5), TotalSizeBytes = 5_100_000_000, IsActive = true };
        var se120 = new GameVersion { GameId = spaceExplorer.Id, VersionNumber = "1.2.0", ReleasedAt = DateTime.UtcNow.AddMonths(-4), TotalSizeBytes = 5_200_000_000, IsActive = true };
        var se130 = new GameVersion { GameId = spaceExplorer.Id, VersionNumber = "1.3.0", ReleasedAt = DateTime.UtcNow.AddMonths(-2), TotalSizeBytes = 5_500_000_000, IsActive = true };
        var se140 = new GameVersion { GameId = spaceExplorer.Id, VersionNumber = "1.4.0", ReleasedAt = DateTime.UtcNow.AddDays(-14), TotalSizeBytes = 5_600_000_000, IsActive = true };

        // Dragon Quest versions
        var dq100 = new GameVersion { GameId = dragonQuest.Id, VersionNumber = "1.0.0", ReleasedAt = DateTime.UtcNow.AddMonths(-3), TotalSizeBytes = 8_000_000_000, IsActive = true };
        var dq101 = new GameVersion { GameId = dragonQuest.Id, VersionNumber = "1.0.1", ReleasedAt = DateTime.UtcNow.AddMonths(-2), TotalSizeBytes = 8_050_000_000, IsActive = true };
        var dq110 = new GameVersion { GameId = dragonQuest.Id, VersionNumber = "1.1.0", ReleasedAt = DateTime.UtcNow.AddMonths(-1), TotalSizeBytes = 8_500_000_000, IsActive = true };

        // Racing Thunder versions
        var rt100 = new GameVersion { GameId = racingThunder.Id, VersionNumber = "1.0.0", ReleasedAt = DateTime.UtcNow.AddMonths(-1), TotalSizeBytes = 3_000_000_000, IsActive = true };
        var rt101 = new GameVersion { GameId = racingThunder.Id, VersionNumber = "1.0.1", ReleasedAt = DateTime.UtcNow.AddDays(-7), TotalSizeBytes = 3_020_000_000, IsActive = true };

        db.GameVersions.AddRange(se100, se110, se120, se130, se140, dq100, dq101, dq110, rt100, rt101);
        await db.SaveChangesAsync();

        // Space Explorer patches - interesting graph for testing optimization
        // Direct path 1.0 -> 1.4 is large (800MB)
        // Incremental path 1.0 -> 1.1 -> 1.2 -> 1.3 -> 1.4 is smaller total (150+120+180+100 = 550MB)
        // But there's also 1.0 -> 1.2 (200MB) and 1.2 -> 1.4 (250MB) = 450MB (optimal)
        var sePatches = new List<PatchFile>
        {
            new() { FromVersionId = se100.Id, ToVersionId = se140.Id, FileName = "se_1.0.0_to_1.4.0.patch", SizeBytes = 800_000_000, Checksum = "abc123", CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { FromVersionId = se100.Id, ToVersionId = se110.Id, FileName = "se_1.0.0_to_1.1.0.patch", SizeBytes = 150_000_000, Checksum = "def456", CreatedAt = DateTime.UtcNow.AddMonths(-5) },
            new() { FromVersionId = se110.Id, ToVersionId = se120.Id, FileName = "se_1.1.0_to_1.2.0.patch", SizeBytes = 120_000_000, Checksum = "ghi789", CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { FromVersionId = se120.Id, ToVersionId = se130.Id, FileName = "se_1.2.0_to_1.3.0.patch", SizeBytes = 180_000_000, Checksum = "jkl012", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { FromVersionId = se130.Id, ToVersionId = se140.Id, FileName = "se_1.3.0_to_1.4.0.patch", SizeBytes = 100_000_000, Checksum = "mno345", CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { FromVersionId = se100.Id, ToVersionId = se120.Id, FileName = "se_1.0.0_to_1.2.0.patch", SizeBytes = 200_000_000, Checksum = "pqr678", CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { FromVersionId = se120.Id, ToVersionId = se140.Id, FileName = "se_1.2.0_to_1.4.0.patch", SizeBytes = 250_000_000, Checksum = "stu901", CreatedAt = DateTime.UtcNow.AddDays(-14) },
        };

        // Dragon Quest patches - simpler linear chain
        var dqPatches = new List<PatchFile>
        {
            new() { FromVersionId = dq100.Id, ToVersionId = dq101.Id, FileName = "dq_1.0.0_to_1.0.1.patch", SizeBytes = 50_000_000, Checksum = "dq1abc", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { FromVersionId = dq101.Id, ToVersionId = dq110.Id, FileName = "dq_1.0.1_to_1.1.0.patch", SizeBytes = 450_000_000, Checksum = "dq2def", CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new() { FromVersionId = dq100.Id, ToVersionId = dq110.Id, FileName = "dq_1.0.0_to_1.1.0.patch", SizeBytes = 480_000_000, Checksum = "dq3ghi", CreatedAt = DateTime.UtcNow.AddMonths(-1) },
        };

        // Racing Thunder patches
        var rtPatches = new List<PatchFile>
        {
            new() { FromVersionId = rt100.Id, ToVersionId = rt101.Id, FileName = "rt_1.0.0_to_1.0.1.patch", SizeBytes = 25_000_000, Checksum = "rt1abc", CreatedAt = DateTime.UtcNow.AddDays(-7) },
        };

        db.PatchFiles.AddRange(sePatches);
        db.PatchFiles.AddRange(dqPatches);
        db.PatchFiles.AddRange(rtPatches);
        await db.SaveChangesAsync();

        // Add some sample download records
        var downloads = new List<DownloadRecord>
        {
            new() { PatchFileId = sePatches[1].Id, StartedAt = DateTime.UtcNow.AddDays(-10), CompletedAt = DateTime.UtcNow.AddDays(-10).AddMinutes(5), ClientIp = "192.168.1.100", Status = DownloadStatus.Completed },
            new() { PatchFileId = sePatches[1].Id, StartedAt = DateTime.UtcNow.AddDays(-9), CompletedAt = DateTime.UtcNow.AddDays(-9).AddMinutes(4), ClientIp = "192.168.1.101", Status = DownloadStatus.Completed },
            new() { PatchFileId = sePatches[2].Id, StartedAt = DateTime.UtcNow.AddDays(-8), CompletedAt = DateTime.UtcNow.AddDays(-8).AddMinutes(3), ClientIp = "192.168.1.100", Status = DownloadStatus.Completed },
            new() { PatchFileId = dqPatches[0].Id, StartedAt = DateTime.UtcNow.AddDays(-5), CompletedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(2), ClientIp = "10.0.0.50", Status = DownloadStatus.Completed },
            new() { PatchFileId = dqPatches[1].Id, StartedAt = DateTime.UtcNow.AddDays(-3), ClientIp = "10.0.0.51", Status = DownloadStatus.Failed },
            new() { PatchFileId = rtPatches[0].Id, StartedAt = DateTime.UtcNow.AddHours(-2), ClientIp = "172.16.0.10", Status = DownloadStatus.InProgress },
        };

        db.DownloadRecords.AddRange(downloads);
        await db.SaveChangesAsync();
    }
}
