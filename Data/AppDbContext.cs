using Microsoft.EntityFrameworkCore;
using XlerionLeaderboardAPI.Models;

namespace XlerionLeaderboardAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<PlayerAchievement> PlayerAchievements => Set<PlayerAchievement>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Username).HasMaxLength(50).IsRequired();
            e.Property(p => p.DisplayName).HasMaxLength(100).IsRequired();
            e.Property(p => p.Region).HasMaxLength(20).HasDefaultValue("GLOBAL");
            e.HasIndex(p => p.Username).IsUnique();
        });

        model.Entity<Score>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.GameId).HasMaxLength(50).IsRequired();
            e.HasIndex(s => new { s.GameId, s.Value });
            e.HasIndex(s => new { s.PlayerId, s.GameId });
            e.HasOne(s => s.Player)
             .WithMany(p => p.Scores)
             .HasForeignKey(s => s.PlayerId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        model.Entity<Achievement>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.GameId).HasMaxLength(50).IsRequired();
            e.Property(a => a.Code).HasMaxLength(50).IsRequired();
            e.Property(a => a.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(a => new { a.GameId, a.Code }).IsUnique();
        });

        model.Entity<PlayerAchievement>(e =>
        {
            e.HasKey(pa => new { pa.PlayerId, pa.AchievementId });
            e.HasOne(pa => pa.Player)
             .WithMany(p => p.PlayerAchievements)
             .HasForeignKey(pa => pa.PlayerId);
            e.HasOne(pa => pa.Achievement)
             .WithMany(a => a.PlayerAchievements)
             .HasForeignKey(pa => pa.AchievementId);
        });
    }
}

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext db)
    {
        // Idempotent seeding: add missing players and achievements without duplicating

        var defaultPlayers = new[]
        {
            new Models.Player { Username = "xlerion_dev", DisplayName = "Xlerion", Region = "LATAM" },
            new Models.Player { Username = "alpha_player", DisplayName = "AlphaOne", Region = "NA" },
            new Models.Player { Username = "beta_runner", DisplayName = "BetaRun", Region = "EU" },
        };

        foreach (var p in defaultPlayers)
        {
            if (!db.Players.Any(x => x.Username == p.Username))
            {
                db.Players.Add(p);
            }
        }

        await db.SaveChangesAsync();

        var defaultAchievements = new[]
        {
            new Models.Achievement { GameId = "xlerion-arena", Code = "FIRST_BLOOD", Name = "First Blood", Description = "Win your first match", Points = 10 },
            new Models.Achievement { GameId = "xlerion-arena", Code = "CENTURION", Name = "Centurion", Description = "Score 100+ in one session", Points = 50 },
            new Models.Achievement { GameId = "xlerion-arena", Code = "LEGEND", Name = "Legendary", Description = "Reach the global top 10", Points = 200 },
        };

        foreach (var a in defaultAchievements)
        {
            if (!db.Achievements.Any(x => x.GameId == a.GameId && x.Code == a.Code))
            {
                db.Achievements.Add(a);
            }
        }

        await db.SaveChangesAsync();
    }
}
