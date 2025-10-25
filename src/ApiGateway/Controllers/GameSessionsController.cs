using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD pour les sessions de jeu d'un joueur
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class GameSessionsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public GameSessionsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameSession>>> GetAll()
        => await _db.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Dungeon)
            .ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GameSession>> GetById(Guid id)
    {
      var session = await _db.GameSessions
          .Include(gs => gs.Player)
          .Include(gs => gs.Dungeon)
          .FirstOrDefaultAsync(gs => gs.Id == id);
      return session is null ? NotFound() : Ok(session);
    }

    [HttpPost]
    public async Task<ActionResult<GameSession>> Create([FromBody] GameSession session)
    {
      if (!_db.Users.Any(u => u.Id == session.PlayerId))
        return BadRequest("Le joueur associé n'existe pas.");
      if (!_db.Dungeons.Any(d => d.Id == session.DungeonId))
        return BadRequest("Le donjon associé n'existe pas.");

      _db.GameSessions.Add(session);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] GameSession input)
    {
      if (id != input.Id) return BadRequest();
      var session = await _db.GameSessions.FindAsync(id);
      if (session is null) return NotFound();

      session.StartedAt = input.StartedAt;
      session.EndedAt = input.EndedAt;
      session.CurrentScore = input.CurrentScore;
      session.IsFinished = input.IsFinished;
      session.PlayerId = input.PlayerId;
      session.DungeonId = input.DungeonId;

      await _db.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var session = await _db.GameSessions.FindAsync(id);
      if (session is null) return NotFound();

      _db.GameSessions.Remove(session);
      await _db.SaveChangesAsync();
      return NoContent();
    }
  }
}
