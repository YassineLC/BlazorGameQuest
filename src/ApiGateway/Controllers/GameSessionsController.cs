using ApiGateway.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD pour les sessions de jeu d'un joueur
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Produces("application/json")]
  public class GameSessionsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public GameSessionsController(AppDbContext db) => _db = db;

    [HttpGet]
    [SwaggerOperation(
      Summary = "Liste les sessions de jeu",
      Description = "Retourne toutes les sessions en incluant les informations de joueur et de donjon.",
      OperationId = "GameSessions_GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GameSession>))]
    public async Task<ActionResult<IEnumerable<GameSession>>> GetAll()
        => await _db.GameSessions
        .AsNoTracking()
            .Include(gs => gs.Player)
            .Include(gs => gs.Dungeon)
            .ToListAsync();

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
      Summary = "Récupère une session",
      Description = "Renvoie la session identifiée avec les détails joueur et donjon.",
      OperationId = "GameSessions_GetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GameSession))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameSession>> GetById(Guid id)
    {
      var session = await _db.GameSessions
          .AsNoTracking()
          .Include(gs => gs.Player)
          .Include(gs => gs.Dungeon)
          .FirstOrDefaultAsync(gs => gs.Id == id);
      return session is null ? NotFound() : Ok(session);
    }

    [HttpPost]
    [SwaggerOperation(
      Summary = "Crée une session",
      Description = "Ajoute une nouvelle session après validation des liens joueur et donjon.",
      OperationId = "GameSessions_Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(GameSession))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GameSession>> Create([FromBody] GameSession session)
    {
      if (!await _db.Users.AsNoTracking().AnyAsync(u => u.Id == session.PlayerId))
        return BadRequest("Le joueur associé n'existe pas.");
      if (!await _db.Dungeons.AsNoTracking().AnyAsync(d => d.Id == session.DungeonId))
        return BadRequest("Le donjon associé n'existe pas.");

      _db.GameSessions.Add(session);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = session.Id }, session);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
      Summary = "Met à jour une session",
      Description = "Actualise les informations d'une session existante.",
      OperationId = "GameSessions_Update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
      Summary = "Supprime une session",
      Description = "Efface la session indiquée si elle existe.",
      OperationId = "GameSessions_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
