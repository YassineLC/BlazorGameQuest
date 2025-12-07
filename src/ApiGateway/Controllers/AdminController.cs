using ApiGateway.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// Endpoints admin pour la gestion du tableau de bord
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "admin,Admin")]
  public class AdminController : ControllerBase
  {
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db) => _db = db;

    /// <summary>Retourne les statistiques globales du tableau de bord</summary>
    [HttpGet("dashboard-stats")]
    public async Task<ActionResult<DashboardStats>> GetDashboardStats()
    {
      var totalPlayers = await _db.Users.CountAsync();
      var totalSessions = await _db.GameSessions.CountAsync();
      var totalScores = await _db.Scores.CountAsync();
      var totalDungeons = await _db.Dungeons.CountAsync();
      var averageScore = totalScores > 0 ? (int)await _db.Scores.AverageAsync(s => s.Value) : 0;

      return Ok(new DashboardStats
      {
        TotalPlayers = totalPlayers,
        TotalSessions = totalSessions,
        TotalScores = totalScores,
        TotalDungeons = totalDungeons,
        AverageScore = averageScore,
        LastUpdated = DateTime.UtcNow
      });
    }

    /// <summary>Retourne la liste des joueurs avec statistiques</summary>
    [HttpGet("players")]
    public async Task<ActionResult<IEnumerable<PlayerAdminDto>>> GetPlayers()
    {
      var players = await _db.Users
        .AsNoTracking()
        .Select(u => new PlayerAdminDto
        {
          Id = u.Id,
          Username = u.Username,
          Email = u.Email,
          SessionCount = _db.GameSessions.Count(gs => gs.PlayerId == u.Id),
          AverageScore = _db.Scores.Where(s => s.PlayerId == u.Id).Any()
            ? (int)_db.Scores.Where(s => s.PlayerId == u.Id).Average(s => s.Value)
            : 0,
          IsActive = u.Role == UserRole.Player,
          CreatedAt = DateTime.UtcNow
        })
        .ToListAsync();

      return Ok(players);
    }

    /// <summary>Désactive un joueur</summary>
    [HttpPut("players/{playerId:guid}/disable")]
    public async Task<IActionResult> DisablePlayer(Guid playerId)
    {
      var user = await _db.Users.FindAsync(playerId);
      if (user is null) return NotFound();

      user.Role = UserRole.Admin;
      await _db.SaveChangesAsync();
      return NoContent();
    }

    /// <summary>Réactive un joueur</summary>
    [HttpPut("players/{playerId:guid}/enable")]
    public async Task<IActionResult> EnablePlayer(Guid playerId)
    {
      var user = await _db.Users.FindAsync(playerId);
      if (user is null) return NotFound();

      user.Role = UserRole.Player;
      await _db.SaveChangesAsync();
      return NoContent();
    }

    /// <summary>Retourne les scores avec détails</summary>
    [HttpGet("scores")]
    public async Task<ActionResult<IEnumerable<ScoreAdminDto>>> GetScores()
    {
      var scores = await _db.Scores
        .Include(s => s.Player)
        .AsNoTracking()
        .Select(s => new ScoreAdminDto
        {
          Id = s.Id,
          PlayerName = s.Player!.Username,
          DungeonName = _db.GameSessions
            .Where(gs => gs.Id == s.SessionId)
            .Select(gs => gs.Dungeon!.Name)
            .FirstOrDefault() ?? "Unknown",
          FinalScore = s.Value,
          KillCount = 0,
          TreasureCount = 0,
          TrapAvoidedCount = 0,
          IsBossDefeated = false,
          AchievedAt = s.AchievedAt
        })
        .OrderByDescending(s => s.AchievedAt)
        .ToListAsync();

      return Ok(scores);
    }

    /// <summary>Retourne les statistiques des scores</summary>
    [HttpGet("scores-stats")]
    public async Task<ActionResult<ScoresStatsDto>> GetScoresStats()
    {
      var scores = await _db.Scores.ToListAsync();
      if (scores.Count == 0)
        return Ok(new ScoresStatsDto { AverageScore = 0, MaxScore = 0, TotalScores = 0 });

      return Ok(new ScoresStatsDto
      {
        AverageScore = (int)scores.Average(s => s.Value),
        MaxScore = scores.Max(s => s.Value),
        TotalScores = scores.Sum(s => s.Value)
      });
    }

    /// <summary>Retourne les sessions de jeu</summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<SessionAdminDto>>> GetSessions()
    {
      var sessions = await _db.GameSessions
        .Include(gs => gs.Player)
        .Include(gs => gs.Dungeon)
        .AsNoTracking()
        .Select(gs => new SessionAdminDto
        {
          Id = gs.Id,
          PlayerName = gs.Player!.Username,
          DungeonName = gs.Dungeon!.Name,
          MaxDepthReached = 0,
          FinalScore = gs.CurrentScore,
          IsFinished = gs.IsFinished,
          StartedAt = gs.StartedAt,
          FinishedAt = gs.EndedAt ?? DateTime.UtcNow
        })
        .OrderByDescending(s => s.StartedAt)
        .ToListAsync();

      return Ok(sessions);
    }

    /// <summary>Retourne les classements globaux</summary>
    [HttpGet("leaderboard/global")]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetGlobalLeaderboard(int limit = 10)
    {
      var leaderboard = await _db.Users
        .Where(u => u.Role == UserRole.Player)
        .AsNoTracking()
        .Select(u => new LeaderboardEntryDto
        {
          PlayerId = u.Id,
          PlayerName = u.Username,
          TotalScore = _db.Scores.Where(s => s.PlayerId == u.Id).Sum(s => s.Value),
          SessionCount = _db.GameSessions.Count(gs => gs.PlayerId == u.Id),
          MaxDepthReached = 0,
          LastUpdate = _db.GameSessions.Where(gs => gs.PlayerId == u.Id).Max(gs => (DateTime?)gs.StartedAt) ?? DateTime.UtcNow
        })
        .OrderByDescending(l => l.TotalScore)
        .Take(limit)
        .ToListAsync();

      return Ok(leaderboard);
    }

    /// <summary>Retourne les donjons avec statistiques</summary>
    [HttpGet("dungeons")]
    public async Task<ActionResult<IEnumerable<DungeonAdminDto>>> GetDungeons()
    {
      var dungeons = await _db.Dungeons
        .AsNoTracking()
        .Select(d => new DungeonAdminDto
        {
          Id = d.Id,
          Name = d.Name,
          MaxDepth = d.MaxDepth,
          RoomCount = _db.Rooms.Count(r => r.DungeonId == d.Id),
          EventCount = 0,
          SessionCount = _db.GameSessions.Count(gs => gs.DungeonId == d.Id),
          CreatedAt = DateTime.UtcNow
        })
        .ToListAsync();

      return Ok(dungeons);
    }

    /// <summary>Exporte tous les joueurs en JSON</summary>
    [HttpGet("export/players")]
    public async Task<IActionResult> ExportPlayers()
    {
      var players = await _db.Users.AsNoTracking().ToListAsync();
      return Ok(players);
    }

    /// <summary>Exporte tous les scores en JSON</summary>
    [HttpGet("export/scores")]
    public async Task<IActionResult> ExportScores()
    {
      var scores = await _db.Scores.Include(s => s.Player).AsNoTracking().ToListAsync();
      return Ok(scores);
    }

    /// <summary>Exporte toutes les sessions en JSON</summary>
    [HttpGet("export/sessions")]
    public async Task<IActionResult> ExportSessions()
    {
      var sessions = await _db.GameSessions
        .Include(gs => gs.Player)
        .Include(gs => gs.Dungeon)
        .AsNoTracking()
        .ToListAsync();
      return Ok(sessions);
    }

    /// <summary>Exporte les donjons en JSON</summary>
    [HttpGet("export/dungeons")]
    public async Task<IActionResult> ExportDungeons()
    {
      var dungeons = await _db.Dungeons.AsNoTracking().ToListAsync();
      return Ok(dungeons);
    }
  }

  /// <summary>DTOs pour les endpoints admin</summary>
  public class DashboardStats
  {
    public int TotalPlayers { get; set; }
    public int TotalSessions { get; set; }
    public int TotalScores { get; set; }
    public int TotalDungeons { get; set; }
    public int AverageScore { get; set; }
    public DateTime LastUpdated { get; set; }
  }

  public class PlayerAdminDto
  {
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public int AverageScore { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
  }

  public class ScoreAdminDto
  {
    public Guid Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string DungeonName { get; set; } = string.Empty;
    public int FinalScore { get; set; }
    public int KillCount { get; set; }
    public int TreasureCount { get; set; }
    public int TrapAvoidedCount { get; set; }
    public bool IsBossDefeated { get; set; }
    public DateTime AchievedAt { get; set; }
  }

  public class ScoresStatsDto
  {
    public int AverageScore { get; set; }
    public int MaxScore { get; set; }
    public int TotalScores { get; set; }
  }

  public class SessionAdminDto
  {
    public Guid Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string DungeonName { get; set; } = string.Empty;
    public int MaxDepthReached { get; set; }
    public int FinalScore { get; set; }
    public bool IsFinished { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
  }

  public class LeaderboardEntryDto
  {
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int TotalScore { get; set; }
    public int SessionCount { get; set; }
    public int MaxDepthReached { get; set; }
    public DateTime LastUpdate { get; set; }
  }

  public class DungeonAdminDto
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int MaxDepth { get; set; }
    public int RoomCount { get; set; }
    public int EventCount { get; set; }
    public int SessionCount { get; set; }
    public DateTime CreatedAt { get; set; }
  }
}
