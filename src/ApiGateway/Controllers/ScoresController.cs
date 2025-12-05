using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD pour la gestion des scores des joueurs
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ScoresController : ControllerBase
  {
    private readonly AppDbContext _db;
    public ScoresController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Score>>> GetAll()
    {
      var list = await _db.Scores.Include(s => s.Player).ToListAsync();

      foreach (var s in list)
      {
        if (s.SessionId != null)
        {
          var session = await _db.GameSessions.Include(gs => gs.Dungeon).FirstOrDefaultAsync(gs => gs.Id == s.SessionId.Value);
          s.DungeonName = session?.Dungeon?.Name;
        }
      }

      return Ok(list);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Score>> GetById(Guid id)
    {
      var score = await _db.Scores.Include(s => s.Player).FirstOrDefaultAsync(s => s.Id == id);
      if (score is null) return NotFound();

      if (score.SessionId != null)
      {
        var session = await _db.GameSessions.Include(gs => gs.Dungeon).FirstOrDefaultAsync(gs => gs.Id == score.SessionId.Value);
        score.DungeonName = session?.Dungeon?.Name;
      }

      return Ok(score);
    }

    [HttpGet("player/{playerId:guid}")]
    public async Task<ActionResult<IEnumerable<Score>>> GetByPlayerId(Guid playerId)
    {
      var list = await _db.Scores.Include(s => s.Player).Where(s => s.PlayerId == playerId).ToListAsync();

      foreach (var s in list)
      {
        if (s.SessionId != null)
        {
          var session = await _db.GameSessions.Include(gs => gs.Dungeon).FirstOrDefaultAsync(gs => gs.Id == s.SessionId.Value);
          s.DungeonName = session?.Dungeon?.Name;
        }
      }

      return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<Score>> Create([FromBody] Score score)
    {
      if (!_db.Users.Any(u => u.Id == score.PlayerId))
        return BadRequest("Le joueur associé n'existe pas.");

      _db.Scores.Add(score);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = score.Id }, score);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Score input)
    {
      if (id != input.Id) return BadRequest();
      var score = await _db.Scores.FindAsync(id);
      if (score is null) return NotFound();

      score.Value = input.Value;
      score.AchievedAt = input.AchievedAt;
      score.SessionId = input.SessionId;
      score.PlayerId = input.PlayerId;

      await _db.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var score = await _db.Scores.FindAsync(id);
      if (score is null) return NotFound();

      _db.Scores.Remove(score);
      await _db.SaveChangesAsync();
      return NoContent();
    }
  }
}
