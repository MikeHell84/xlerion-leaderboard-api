namespace XlerionLeaderboardAPI.Models;

public class Player
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Region { get; set; } = "GLOBAL";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<PlayerAchievement> PlayerAchievements { get; set; } = new List<PlayerAchievement>();
}

public class Score
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string GameId { get; set; } = string.Empty;
    public int Value { get; set; }
    public int Level { get; set; }
    public double SessionDurationSeconds { get; set; }
    public string? Metadata { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Player Player { get; set; } = null!;
}

public class Achievement
{
    public int Id { get; set; }
    public string GameId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }

    public ICollection<PlayerAchievement> PlayerAchievements { get; set; } = new List<PlayerAchievement>();
}

public class PlayerAchievement
{
    public int PlayerId { get; set; }
    public int AchievementId { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public Player Player { get; set; } = null!;
    public Achievement Achievement { get; set; } = null!;
}
