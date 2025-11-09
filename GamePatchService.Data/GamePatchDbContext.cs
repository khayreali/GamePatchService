using GamePatchService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GamePatchService.Data;

public class GamePatchDbContext : DbContext
{
    public GamePatchDbContext(DbContextOptions<GamePatchDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<GameVersion> GameVersions => Set<GameVersion>();
    public DbSet<PatchFile> PatchFiles => Set<PatchFile>();
    public DbSet<DownloadRecord> DownloadRecords => Set<DownloadRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Title).IsRequired().HasMaxLength(256);
            entity.Property(g => g.Publisher).HasMaxLength(256);
        });

        modelBuilder.Entity<GameVersion>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.VersionNumber).IsRequired().HasMaxLength(32);

            entity.HasIndex(v => v.VersionNumber);
            entity.HasIndex(v => new { v.GameId, v.VersionNumber }).IsUnique();

            entity.HasOne(v => v.Game)
                .WithMany(g => g.Versions)
                .HasForeignKey(v => v.GameId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PatchFile>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FileName).IsRequired().HasMaxLength(512);
            entity.Property(p => p.Checksum).HasMaxLength(64);

            entity.HasOne(p => p.FromVersion)
                .WithMany(v => v.PatchesFrom)
                .HasForeignKey(p => p.FromVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.ToVersion)
                .WithMany(v => v.PatchesTo)
                .HasForeignKey(p => p.ToVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => new { p.FromVersionId, p.ToVersionId }).IsUnique();
        });

        modelBuilder.Entity<DownloadRecord>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.ClientIp).HasMaxLength(45);

            entity.HasIndex(d => d.Status);

            entity.HasOne(d => d.PatchFile)
                .WithMany(p => p.Downloads)
                .HasForeignKey(d => d.PatchFileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
