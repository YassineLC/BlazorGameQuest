using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class PlayController : ControllerBase
  {
    private readonly AppDbContext _db;
    public PlayController(AppDbContext db) => _db = db;

    /// <summary>
    /// Démarre une session de jeu pour un joueur (ou crée un joueur 'guest' si absent)
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<GameSession>> StartSession([FromBody] StartSessionRequest req)
    {
      Guid playerId;

      if (req.PlayerId != null)
      {
        var p = await _db.Users.FindAsync(req.PlayerId.Value);
        if (p == null) return BadRequest("Player not found");
        playerId = p.Id;
      }
      else
      {
        // Rechercher ou créer un utilisateur invité
        var guest = await _db.Users.FirstOrDefaultAsync(u => u.Username == "guest");
        if (guest == null)
        {
          guest = new User { Username = "guest", Email = "guest@local", Role = UserRole.Player };
          _db.Users.Add(guest);
          await _db.SaveChangesAsync();
        }
        playerId = guest.Id;
      }

      if (!await _db.Dungeons.AnyAsync(d => d.Id == req.DungeonId)) return BadRequest("Dungeon not found");

      var session = new GameSession
      {
        PlayerId = playerId,
        DungeonId = req.DungeonId,
        CurrentScore = 0,
        IsFinished = false,
        StartedAt = DateTime.UtcNow
      };

      _db.GameSessions.Add(session);
      await _db.SaveChangesAsync();

      return CreatedAtAction(null, session);
    }

    /// <summary>
    /// Marque la visite d'une salle par la session (ajoute les points, peut terminer la session)
    /// </summary>
    [HttpPost("visit")] 
    public async Task<ActionResult<VisitResult>> VisitRoom([FromBody] VisitRoomRequest req)
    {
      var session = await _db.GameSessions.FindAsync(req.SessionId);
      if (session == null) return BadRequest("Session not found");

      var room = await _db.Rooms.FindAsync(req.RoomId);
      if (room == null) return BadRequest("Room not found");

      if (session.IsFinished) return BadRequest("Session already finished");

      // Logique simple: visiter la salle rapporte ses PointsReward
      // Les choix pourraient influer; ici on simule un modificateur selon le "Choice" envoyé.
      int points = room.PointsReward;

      var modifier = req.Choice?.ToLower() switch
      {
        "combat" => 1.2, // plus de risques mais plus de points
        "explore" => 1.0,
        "flee" => 0.4,
        _ => 1.0
      };

      // Chance d'échec réduite pour les choix prudents
      var rnd = new Random();
      var difficultyPenalty = room.Difficulty == RoomDifficulty.Hard ? 0.15 : room.Difficulty == RoomDifficulty.Medium ? 0.08 : 0.03;
      var failChance = Math.Clamp(0.05 + difficultyPenalty - (modifier - 1.0) * 0.05, 0.0, 0.9);

      var failed = rnd.NextDouble() < failChance;

      int gained = failed ? (int)Math.Floor(points * modifier * 0.25) : (int)Math.Ceiling(points * modifier);

      session.CurrentScore += gained;

      // Si échec critique sur boss -> fin de session
      if (failed && room.Type == RoomType.Boss)
      {
        session.IsFinished = true;
        session.EndedAt = DateTime.UtcNow;

        // Persister un Score
        var score = new Score { PlayerId = session.PlayerId, Value = session.CurrentScore, SessionId = session.Id };
        _db.Scores.Add(score);
      }

      await _db.SaveChangesAsync();

      var nextIds = room.NextRoomIds;

      var result = new VisitResult
      {
        NewScore = session.CurrentScore,
        IsFinished = session.IsFinished,
        Message = failed ? "Vous avez échoué partiellement et gagnez moins de points." : "Succès! Vous gagnez des points.",
        NextRoomIds = nextIds
      };

      return Ok(result);
    }

    /// <summary>
    /// Termine explicitement la session et enregistre le score
    /// </summary>
    [HttpPost("end")]
    public async Task<IActionResult> EndSession([FromBody] EndSessionRequest req)
    {
      var session = await _db.GameSessions.FindAsync(req.SessionId);
      if (session == null) return BadRequest("Session not found");

      if (!session.IsFinished)
      {
        session.IsFinished = true;
        session.EndedAt = DateTime.UtcNow;

        var score = new Score { PlayerId = session.PlayerId, Value = session.CurrentScore, SessionId = session.Id };
        _db.Scores.Add(score);
        await _db.SaveChangesAsync();
      }

      return NoContent();
    }
  }

  public record StartSessionRequest(Guid? PlayerId, Guid DungeonId);
  public record VisitRoomRequest(Guid SessionId, Guid RoomId, string? Choice);
  public class VisitResult
  {
    public int NewScore { get; set; }
    public bool IsFinished { get; set; }
    public string? Message { get; set; }
    public List<Guid> NextRoomIds { get; set; } = new();
  }
  public record EndSessionRequest(Guid SessionId);
}
