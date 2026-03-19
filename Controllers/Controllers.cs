using Microsoft.AspNetCore.Mvc;
using XlerionLeaderboardAPI.DTOs;
using System.Text.Json;
using XlerionLeaderboardAPI.Services;

namespace XlerionLeaderboardAPI.Controllers;

// ──────────────────────────────────────────────
// SCORES CONTROLLER
// ──────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ScoresController : ControllerBase
{
    private readonly ILeaderboardService _service;
    public ScoresController(ILeaderboardService service) => _service = service;

    /// <summary>Submit a new game score for a player.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitScore([FromBody] ScoreSubmitDto dto)
    {
        if (dto.Value < 0) return BadRequest("Score value cannot be negative.");

        try
        {
            var scoreDto = await _service.SubmitScoreAsync(dto);
            return CreatedAtAction(nameof(SubmitScore), new { id = scoreDto.Id }, scoreDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

// ──────────────────────────────────────────────
// LEADERBOARD CONTROLLER
// ──────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _service;
    public LeaderboardController(ILeaderboardService service) => _service = service;

    /// <summary>Get top players for a specific game. Optionally filter by region.</summary>
    /// <param name="gameId">Game identifier (e.g. xlerion-arena)</param>
    /// <param name="region">Optional region filter: NA, EU, LATAM, GLOBAL</param>
    /// <param name="top">Number of entries to return (default: 10, max: 100)</param>
    [HttpGet("{gameId}")]
    [ProducesResponseType(typeof(IEnumerable<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaderboard(
        string gameId,
        [FromQuery] string? region = null,
        [FromQuery] int top = 10)
    {
        top = Math.Clamp(top, 1, 100);
        var entries = await _service.GetLeaderboardAsync(gameId, region, top);
        return Ok(entries);
    }
}

// ──────────────────────────────────────────────
// PLAYERS CONTROLLER
// ──────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlayersController : ControllerBase
{
    private readonly ILeaderboardService _service;
    public PlayersController(ILeaderboardService service) => _service = service;

    /// <summary>Get full stats for a player: scores, achievements, averages.</summary>
    [HttpGet("{id}/stats")]
    [ProducesResponseType(typeof(PlayerStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlayerStats(int id)
    {
        var stats = await _service.GetPlayerStatsAsync(id);
        return stats is null ? NotFound($"Player {id} not found.") : Ok(stats);
    }

    /// <summary>Create a new player account.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePlayer([FromBody] PlayerCreateDto dto)
    {
        // If username exists, return Conflict with details (id + message)
        var existing = await _service.GetPlayerByUsernameAsync(dto.Username);
        if (existing is not null)
        {
            return Conflict(new { message = $"Username '{dto.Username}' is already taken.", existingPlayerId = existing.Id });
        }

        var player = await _service.CreatePlayerAsync(dto);
        return CreatedAtAction(nameof(GetPlayerStats), new { id = player.Id }, player);
    }
}

// ──────────────────────────────────────────────
// ACHIEVEMENTS CONTROLLER
// ──────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AchievementsController : ControllerBase
{
    private readonly ILeaderboardService _service;
    public AchievementsController(ILeaderboardService service) => _service = service;

    /// <summary>Unlock an achievement for a player.</summary>
    [HttpPost("unlock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlockAchievement([FromBody] JsonElement payload)
    {
        AchievementUnlockDto dto;
        try
        {
            // Swagger UI or other clients occasionally send the JSON as a string (e.g. "{\"playerId\":1,...}")
            // Accept either a raw object or a JSON string containing the object for robustness.
            if (payload.ValueKind == JsonValueKind.String)
            {
                var inner = payload.GetString();
                dto = JsonSerializer.Deserialize<AchievementUnlockDto>(inner!);
            }
            else
            {
                dto = JsonSerializer.Deserialize<AchievementUnlockDto>(payload.GetRawText());
            }
        }
        catch (JsonException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid JSON", Detail = ex.Message });
        }

        if (dto is null)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid request", Detail = "Request body could not be parsed as AchievementUnlockDto." });
        }

        var result = await _service.UnlockAchievementAsync(dto);

        return result switch
        {
            XlerionLeaderboardAPI.Services.UnlockResult.Success => Ok(new { message = $"Achievement '{dto.AchievementCode}' unlocked successfully." }),
            XlerionLeaderboardAPI.Services.UnlockResult.AlreadyUnlocked => Conflict(new { message = $"Achievement '{dto.AchievementCode}' already unlocked for player {dto.PlayerId}." }),
            XlerionLeaderboardAPI.Services.UnlockResult.NotFound => NotFound(new { message = $"Achievement '{dto.AchievementCode}' not found for game '{dto.GameId}'." }),
            _ => StatusCode(500, new { message = "Unknown result." })
        };
    }
}
