using ApiGateway.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD  pour la gestion des pièges (Traps) d'une salle
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Produces("application/json")]
  public class TrapsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public TrapsController(AppDbContext db) => _db = db;

    /// <summary>Retourne tous les pièges existants </summary>
    [HttpGet]
    [SwaggerOperation(
      Summary = "Liste les pièges",
      Description = "Retourne l'ensemble des pièges avec leur salle associée.",
      OperationId = "Traps_GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Trap>))]
    public async Task<ActionResult<IEnumerable<Trap>>> GetAll()
      => await _db.Traps
        .AsNoTracking()
        .Include(t => t.Room)
        .ToListAsync();

    /// <summary>Retourne un piège par son Id </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
      Summary = "Récupère un piège",
      Description = "Renvoie le piège correspondant à l'identifiant fourni.",
      OperationId = "Traps_GetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Trap))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Trap>> GetById(Guid id)
    {
      var trap = await _db.Traps
          .AsNoTracking()
          .FirstOrDefaultAsync(t => t.Id == id);
      return trap is null ? NotFound() : Ok(trap);
    }

    /// <summary>Crée un nouveau piège </summary>
    [HttpPost]
    [SwaggerOperation(
      Summary = "Crée un piège",
      Description = "Ajoute un nouveau piège à une salle.",
      OperationId = "Traps_Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Trap))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Trap>> Create([FromBody] Trap trap)
    {
      if (!await _db.Rooms.AsNoTracking().AnyAsync(r => r.Id == trap.RoomId))
        return BadRequest("La salle associée n'existe pas.");

      _db.Traps.Add(trap);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = trap.Id }, trap);
    }

    /// <summary>Met à jour un piège existant </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
      Summary = "Met à jour un piège",
      Description = "Actualise les propriétés d'un piège existant.",
      OperationId = "Traps_Update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Trap input)
    {
      if (id != input.Id) return BadRequest();
      var trap = await _db.Traps.FindAsync(id);
      if (trap is null) return NotFound();

      trap.Type = input.Type;
      trap.Damage = input.Damage;
      trap.ChanceToTriggerPercent = input.ChanceToTriggerPercent;
      trap.CanBeDisarmed = input.CanBeDisarmed;
      trap.IsDisarmed = input.IsDisarmed;
      trap.RoomId = input.RoomId;

      await _db.SaveChangesAsync();
      return NoContent();
    }

    /// <summary>Supprime un piège </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
      Summary = "Supprime un piège",
      Description = "Supprime le piège correspondant à l'identifiant fourni.",
      OperationId = "Traps_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
      var trap = await _db.Traps.FindAsync(id);
      if (trap is null) return NotFound();

      _db.Traps.Remove(trap);
      await _db.SaveChangesAsync();
      return NoContent();
    }
  }
}
