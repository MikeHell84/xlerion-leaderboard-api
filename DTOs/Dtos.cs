namespace XlerionLeaderboardAPI.DTOs;

public record ScoreSubmitDto(
    int PlayerId,
    string GameId,
    int Value,
    int Level,
    double SessionDurationSeconds,
    string? Metadata
);

public record ScoreDto(
    int Id,
    int PlayerId,
    string GameId,
    int Value,
    int Level,
    double SessionDurationSeconds,
    string? Metadata,
    DateTime RecordedAt
);

public record LeaderboardEntryDto(
    int Rank,
    int PlayerId,
    string Username,
    string DisplayName,
    string Region,
    int BestScore,
    int Level,
    DateTime RecordedAt
);

public record PlayerStatsDto(
    int PlayerId,
    string Username,
    string DisplayName,
    string Region,
    int TotalGames,
    int BestScore,
    double AverageScore,
    int TotalAchievements,
    int AchievementPoints,
    List<RecentScoreDto> RecentScores,
    List<AchievementDto> Achievements
);

public record RecentScoreDto(
    string GameId,
    int Value,
    int Level,
    DateTime RecordedAt
);

public record AchievementUnlockDto(
    int PlayerId,
    string GameId,
    string AchievementCode
);

public record AchievementDto(
    string Code,
    string Name,
    string Description,
    int Points,
    DateTime UnlockedAt
);

public record PlayerCreateDto(
    string Username,
    string DisplayName,
    string Region
);
