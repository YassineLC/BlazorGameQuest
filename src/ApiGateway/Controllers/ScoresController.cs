using System.Linq;
using ApiGateway.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD pour la gestion des scores des joueurs
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Produces("application/json")]
  public class ScoresController : ControllerBase
  {
    private readonly AppDbContext _db;
    public ScoresController(AppDbContext db) => _db = db;

    [HttpGet]
    [SwaggerOperation(
      Summary = "Liste tous les scores",
      Description = "Retourne la liste complète des scores en incluant le joueur et le donjon associé lorsque disponible.",
      OperationId = "Scores_GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Score>))]
    public async Task<ActionResult<IEnumerable<Score>>> GetAll()
    {
      var list = await _db.Scores
          .AsNoTracking()
          .Include(s => s.Player)
          .ToListAsync();

      await PopulateDungeonNamesAsync(list);

      return Ok(list);
    }

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
      Summary = "Récupère un score par identifiant",
      Description = "Renvoie un score enrichi des informations joueur et donjon.",
      OperationId = "Scores_GetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Score))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Score>> GetById(Guid id)
    {
      var score = await _db.Scores
          .AsNoTracking()
          .Include(s => s.Player)
          .FirstOrDefaultAsync(s => s.Id == id);
      if (score is null) return NotFound();

      await PopulateDungeonNamesAsync(new[] { score });

      return Ok(score);
    }

    [HttpGet("player/{playerId:guid}")]
    [SwaggerOperation(
      Summary = "Liste les scores d'un joueur",
      Description = "Filtre les scores par joueur et renvoie les informations enrichies associées.",
      OperationId = "Scores_GetByPlayerId")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Score>))]
    public async Task<ActionResult<IEnumerable<Score>>> GetByPlayerId(Guid playerId)
    {
      var list = await _db.Scores
          .AsNoTracking()
          .Include(s => s.Player)
          .Where(s => s.PlayerId == playerId)
          .ToListAsync();

      await PopulateDungeonNamesAsync(list);

      return Ok(list);
    }

    [HttpPost]
    [SwaggerOperation(
      Summary = "Crée un score",
      Description = "Persiste un nouveau score pour un joueur donné.",
      OperationId = "Scores_Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Score))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Score>> Create([FromBody] Score score)
    {
      if (!await _db.Users.AsNoTracking().AnyAsync(u => u.Id == score.PlayerId))
        return BadRequest("Le joueur associé n'existe pas.");

      _db.Scores.Add(score);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = score.Id }, score);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
      Summary = "Met à jour un score",
      Description = "Actualise les informations d'un score existant.",
      OperationId = "Scores_Update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
      Summary = "Supprime un score",
      Description = "Efface un score existant à partir de son identifiant.",
      OperationId = "Scores_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
      var score = await _db.Scores.FindAsync(id);
      if (score is null) return NotFound();

      _db.Scores.Remove(score);
      await _db.SaveChangesAsync();
      return NoContent();
    }

    private async Task PopulateDungeonNamesAsync(IEnumerable<Score> scores)
    {
      var sessionIds = scores
          .Where(s => s.SessionId.HasValue)
          .Select(s => s.SessionId!.Value)
          .Distinct()
          .ToList();

      if (sessionIds.Count == 0)
      {
        return;
      }

      var sessions = await _db.GameSessions
          .AsNoTracking()
          .Include(gs => gs.Dungeon)
          .Where(gs => sessionIds.Contains(gs.Id))
          .ToListAsync();

      var dungeonLookup = sessions.ToDictionary(gs => gs.Id, gs => gs.Dungeon?.Name);

      foreach (var score in scores)
      {
        if (score.SessionId is { } sessionId && dungeonLookup.TryGetValue(sessionId, out var dungeonName))
        {
          score.DungeonName = dungeonName;
        }
      }
    }
  }
}

