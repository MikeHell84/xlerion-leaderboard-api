using Microsoft.EntityFrameworkCore;
using XlerionLeaderboardAPI.Data;
using XlerionLeaderboardAPI.DTOs;
using XlerionLeaderboardAPI.Models;

namespace XlerionLeaderboardAPI.Services;

public enum UnlockResult { Success, AlreadyUnlocked, NotFound }

public interface ILeaderboardService
{
    Task<XlerionLeaderboardAPI.DTOs.ScoreDto> SubmitScoreAsync(ScoreSubmitDto dto);
    Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(string gameId, string? region, int top);
    Task<PlayerStatsDto?> GetPlayerStatsAsync(int playerId);
    Task<UnlockResult> UnlockAchievementAsync(AchievementUnlockDto dto);
    Task<Player> CreatePlayerAsync(PlayerCreateDto dto);
    Task<Player?> GetPlayerByUsernameAsync(string username);
}

public class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _db;
    private readonly ILogger<LeaderboardService> _logger;

    public LeaderboardService(AppDbContext db, ILogger<LeaderboardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<XlerionLeaderboardAPI.DTOs.ScoreDto> SubmitScoreAsync(ScoreSubmitDto dto)
    {
        _logger.LogInformation("Submitting score for PlayerId={PlayerId} GameId={GameId} Value={Value}", dto.PlayerId, dto.GameId, dto.Value);

        var player = await _db.Players.FindAsync(dto.PlayerId)
            ?? throw new KeyNotFoundException($"Player {dto.PlayerId} not found.");

        var score = new Score
        {
            PlayerId = dto.PlayerId,
            GameId = dto.GameId,
            Value = dto.Value,
            Level = dto.Level,
            SessionDurationSeconds = dto.SessionDurationSeconds,
            Metadata = dto.Metadata,
            RecordedAt = DateTime.UtcNow
        };

        _db.Scores.Add(score);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Score saved with Id={ScoreId}", score.Id);

        await CheckAutoAchievementsAsync(player, score);

        // map to DTO to avoid returning EF entities with navigation cycles
        var scoreDto = new XlerionLeaderboardAPI.DTOs.ScoreDto(
            score.Id,
            score.PlayerId,
            score.GameId,
            score.Value,
            score.Level,
            score.SessionDurationSeconds,
            score.Metadata,
            score.RecordedAt
        );

        return scoreDto;
    }

    public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(
        string gameId, string? region, int top)
    {
        _logger.LogInformation("Fetching leaderboard for GameId={GameId} Region={Region} Top={Top}", gameId, region ?? "GLOBAL", top);

        var query = _db.Scores
            .Include(s => s.Player)
            .Where(s => s.GameId == gameId && s.Player.IsActive);

        if (!string.IsNullOrEmpty(region) && region != "GLOBAL")
            query = query.Where(s => s.Player.Region == region);

        var best = await query
            .GroupBy(s => s.PlayerId)
            .Select(g => new
            {
                PlayerId = g.Key,
                BestScore = g.Max(s => s.Value),
                Level = g.OrderByDescending(s => s.Value).First().Level,
                RecordedAt = g.OrderByDescending(s => s.Value).First().RecordedAt,
                Username = g.First().Player.Username,
                DisplayName = g.First().Player.DisplayName,
                Region = g.First().Player.Region
            })
            .OrderByDescending(x => x.BestScore)
            .Take(top)
            .ToListAsync();

        var result = best.Select((x, i) => new LeaderboardEntryDto(
            Rank: i + 1,
            PlayerId: x.PlayerId,
            Username: x.Username,
            DisplayName: x.DisplayName,
            Region: x.Region,
            BestScore: x.BestScore,
            Level: x.Level,
            RecordedAt: x.RecordedAt
        )).ToList();

        _logger.LogInformation("Returning {Count} leaderboard entries for GameId={GameId}", result.Count, gameId);

        return result;
    }

    public async Task<PlayerStatsDto?> GetPlayerStatsAsync(int playerId)
    {
        _logger.LogInformation("Getting stats for PlayerId={PlayerId}", playerId);

        var player = await _db.Players
            .Include(p => p.Scores)
            .Include(p => p.PlayerAchievements)
                .ThenInclude(pa => pa.Achievement)
            .FirstOrDefaultAsync(p => p.Id == playerId);

        if (player is null) return null;

        var scores = player.Scores.ToList();
        var achievements = player.PlayerAchievements.ToList();

        return new PlayerStatsDto(
            PlayerId: player.Id,
            Username: player.Username,
            DisplayName: player.DisplayName,
            Region: player.Region,
            TotalGames: scores.Count,
            BestScore: scores.Count > 0 ? scores.Max(s => s.Value) : 0,
            AverageScore: scores.Count > 0 ? scores.Average(s => s.Value) : 0,
            TotalAchievements: achievements.Count,
            AchievementPoints: achievements.Sum(pa => pa.Achievement.Points),
            RecentScores: scores
                .OrderByDescending(s => s.RecordedAt)
                .Take(10)
                .Select(s => new RecentScoreDto(s.GameId, s.Value, s.Level, s.RecordedAt))
                .ToList(),
            Achievements: achievements
                .Select(pa => new AchievementDto(
                    pa.Achievement.Code,
                    pa.Achievement.Name,
                    pa.Achievement.Description,
                    pa.Achievement.Points,
                    pa.UnlockedAt))
                .ToList()
        );
    }

    public async Task<UnlockResult> UnlockAchievementAsync(AchievementUnlockDto dto)
    {
        _logger.LogInformation("Unlock achievement request: PlayerId={PlayerId} GameId={GameId} Code={Code}", dto.PlayerId, dto.GameId, dto.AchievementCode);

        var achievement = await _db.Achievements
            .FirstOrDefaultAsync(a => a.GameId == dto.GameId && a.Code == dto.AchievementCode);

        if (achievement is null)
        {
            _logger.LogInformation("Achievement {Code} not found for Game {GameId}", dto.AchievementCode, dto.GameId);
            return UnlockResult.NotFound;
        }

        var alreadyUnlocked = await _db.PlayerAchievements
            .AnyAsync(pa => pa.PlayerId == dto.PlayerId && pa.AchievementId == achievement.Id);

        if (alreadyUnlocked)
        {
            _logger.LogInformation("Achievement {Code} already unlocked for Player {PlayerId}", dto.AchievementCode, dto.PlayerId);
            return UnlockResult.AlreadyUnlocked;
        }

        _db.PlayerAchievements.Add(new PlayerAchievement
        {
            PlayerId = dto.PlayerId,
            AchievementId = achievement.Id,
            UnlockedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Achievement {Code} unlocked for Player {PlayerId}", dto.AchievementCode, dto.PlayerId);
        return UnlockResult.Success;
    }

    public async Task<Player> CreatePlayerAsync(PlayerCreateDto dto)
    {
        _logger.LogInformation("Creating player Username={Username}", dto.Username);

        if (await _db.Players.AnyAsync(p => p.Username == dto.Username))
            throw new InvalidOperationException($"Username '{dto.Username}' is already taken.");

        var player = new Player
        {
            Username = dto.Username,
            DisplayName = dto.DisplayName,
            Region = dto.Region,
            CreatedAt = DateTime.UtcNow
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created player Id={PlayerId} Username={Username}", player.Id, player.Username);
        return player;
    }

    public async Task<Player?> GetPlayerByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        return await _db.Players.FirstOrDefaultAsync(p => p.Username == username);
    }

    private async Task CheckAutoAchievementsAsync(Player player, Score score)
    {
        if (score.Value >= 100)
        {
            await UnlockAchievementAsync(new AchievementUnlockDto(
                player.Id, score.GameId, "CENTURION"));
        }
    }
}
