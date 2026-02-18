using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data;

public static class SeedData
{
    public static async Task InitializeAsync(GamePatchDbContext db)
    {
        if (await db.Games.AnyAsync())
            return;

        var crabNebula = new Game
        {
            Title = "Crab Nebula",
            Publisher = "Moondog Interactive",
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };

        var frostpunk = new Game
        {
            Title = "Frostpunk Tactics",
            Publisher = "Grizzly Bear Labs",
            CreatedAt = DateTime.UtcNow.AddMonths(-3)
        };

        var turboSnails = new Game
        {
            Title = "Turbo Snails",
            Publisher = "Couch Potato Games",
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        db.Games.AddRange(crabNebula, frostpunk, turboSnails);
        await db.SaveChangesAsync();

        // Crab Nebula versions
        var cn100 = new GameVersion { GameId = crabNebula.Id, VersionNumber = "1.0.0", ReleasedAt = DateTime.UtcNow.AddMonths(-6), TotalSizeBytes = 5_000_000_000, IsActive = true };
        var cn110 = new GameVersion { GameId = crabNebula.Id, VersionNumber = "1.1.0", ReleasedAt = DateTime.UtcNow.AddMonths(-5), TotalSizeBytes = 5_100_000_000, IsActive = true };
        var cn120 = new GameVersion { GameId = crabNebula.Id, VersionNumber = "1.2.0", ReleasedAt = DateTime.UtcNow.AddMonths(-4), TotalSizeBytes = 5_200_000_000, IsActive = true };
        var cn130 = new GameVersion { GameId = crabNebula.Id, VersionNumber = "1.3.0", ReleasedAt = DateTime.UtcNow.AddMonths(-2), TotalSizeBytes = 5_500_000_000, IsActive = true };
        var cn140 = new GameVersion { GameId = crabNebula.Id, VersionNumber = "1.4.0", ReleasedAt = DateTime.UtcNow.AddDays(-14), TotalSizeBytes = 5_600_000_000, IsActive = true };

        // Frostpunk versions
        var fp100 = new GameVersion { GameId = frostpunk.Id, VersionNumber = "1.0.0", ReleasedAt = DateTime.UtcNow.AddMonths(-3), TotalSizeBytes = 8_000_000_000, IsActive = true };
        var fp101 = new GameVersion { GameId = frostpunk.Id, VersionNumber = "1.0.1", ReleasedAt = DateTime.UtcNow.AddMonths(-2), TotalSizeBytes = 8_050_000_000, IsActive = true };
        var fp110 = new GameVersion { GameId = frostpunk.Id, VersionNumber = "1.1.0", ReleasedAt = DateTime.UtcNow.AddMonths(-1), TotalSizeBytes = 8_500_000_000, IsActive = true };

        // Turbo Snails versions
        var ts100 = new GameVersion { GameId = turboSnails.Id, VersionNumber = "1.0.0", ReleasedAt = DateTime.UtcNow.AddMonths(-1), TotalSizeBytes = 3_000_000_000, IsActive = true };
        var ts101 = new GameVersion { GameId = turboSnails.Id, VersionNumber = "1.0.1", ReleasedAt = DateTime.UtcNow.AddDays(-7), TotalSizeBytes = 3_020_000_000, IsActive = true };

        db.GameVersions.AddRange(cn100, cn110, cn120, cn130, cn140, fp100, fp101, fp110, ts100, ts101);
        await db.SaveChangesAsync();

        // Crab Nebula patches — graph with multiple paths for optimization testing
        var cnPatches = new List<PatchFile>
        {
            new() { FromVersionId = cn100.Id, ToVersionId = cn140.Id, FileName = "cn_1.0.0_to_1.4.0.patch", SizeBytes = 800_000_000, Checksum = "9f3a7c2e", CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { FromVersionId = cn100.Id, ToVersionId = cn110.Id, FileName = "cn_1.0.0_to_1.1.0.patch", SizeBytes = 150_000_000, Checksum = "d41b08fa", CreatedAt = DateTime.UtcNow.AddMonths(-5) },
            new() { FromVersionId = cn110.Id, ToVersionId = cn120.Id, FileName = "cn_1.1.0_to_1.2.0.patch", SizeBytes = 120_000_000, Checksum = "7e2f91cb", CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { FromVersionId = cn120.Id, ToVersionId = cn130.Id, FileName = "cn_1.2.0_to_1.3.0.patch", SizeBytes = 180_000_000, Checksum = "b50ca63d", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { FromVersionId = cn130.Id, ToVersionId = cn140.Id, FileName = "cn_1.3.0_to_1.4.0.patch", SizeBytes = 100_000_000, Checksum = "2d8e4f17", CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { FromVersionId = cn100.Id, ToVersionId = cn120.Id, FileName = "cn_1.0.0_to_1.2.0.patch", SizeBytes = 200_000_000, Checksum = "c83f20b6", CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new() { FromVersionId = cn120.Id, ToVersionId = cn140.Id, FileName = "cn_1.2.0_to_1.4.0.patch", SizeBytes = 250_000_000, Checksum = "5a19d4e8", CreatedAt = DateTime.UtcNow.AddDays(-14) },
        };

        // Frostpunk — linear chain + one skip
        var fpPatches = new List<PatchFile>
        {
            new() { FromVersionId = fp100.Id, ToVersionId = fp101.Id, FileName = "fp_1.0.0_to_1.0.1.patch", SizeBytes = 50_000_000, Checksum = "e7b3d912", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new() { FromVersionId = fp101.Id, ToVersionId = fp110.Id, FileName = "fp_1.0.1_to_1.1.0.patch", SizeBytes = 450_000_000, Checksum = "41f6a0c8", CreatedAt = DateTime.UtcNow.AddMonths(-1) },
            new() { FromVersionId = fp100.Id, ToVersionId = fp110.Id, FileName = "fp_1.0.0_to_1.1.0.patch", SizeBytes = 480_000_000, Checksum = "8c2de57b", CreatedAt = DateTime.UtcNow.AddMonths(-1) },
        };

        // Turbo Snails — single hotfix
        var tsPatches = new List<PatchFile>
        {
            new() { FromVersionId = ts100.Id, ToVersionId = ts101.Id, FileName = "ts_1.0.0_to_1.0.1.patch", SizeBytes = 25_000_000, Checksum = "f04b7e39", CreatedAt = DateTime.UtcNow.AddDays(-7) },
        };

        db.PatchFiles.AddRange(cnPatches);
        db.PatchFiles.AddRange(fpPatches);
        db.PatchFiles.AddRange(tsPatches);
        await db.SaveChangesAsync();

        var downloads = new List<DownloadRecord>
        {
            new() { PatchFileId = cnPatches[1].Id, StartedAt = DateTime.UtcNow.AddDays(-10), CompletedAt = DateTime.UtcNow.AddDays(-10).AddMinutes(5), ClientIp = "192.168.1.100", Status = DownloadStatus.Completed },
            new() { PatchFileId = cnPatches[1].Id, StartedAt = DateTime.UtcNow.AddDays(-9), CompletedAt = DateTime.UtcNow.AddDays(-9).AddMinutes(4), ClientIp = "192.168.1.101", Status = DownloadStatus.Completed },
            new() { PatchFileId = cnPatches[2].Id, StartedAt = DateTime.UtcNow.AddDays(-8), CompletedAt = DateTime.UtcNow.AddDays(-8).AddMinutes(3), ClientIp = "192.168.1.100", Status = DownloadStatus.Completed },
            new() { PatchFileId = fpPatches[0].Id, StartedAt = DateTime.UtcNow.AddDays(-5), CompletedAt = DateTime.UtcNow.AddDays(-5).AddMinutes(2), ClientIp = "10.0.0.50", Status = DownloadStatus.Completed },
            new() { PatchFileId = fpPatches[1].Id, StartedAt = DateTime.UtcNow.AddDays(-3), ClientIp = "10.0.0.51", Status = DownloadStatus.Failed },
            new() { PatchFileId = tsPatches[0].Id, StartedAt = DateTime.UtcNow.AddHours(-2), ClientIp = "172.16.0.10", Status = DownloadStatus.InProgress },
        };

        db.DownloadRecords.AddRange(downloads);
        await db.SaveChangesAsync();
    }
}
