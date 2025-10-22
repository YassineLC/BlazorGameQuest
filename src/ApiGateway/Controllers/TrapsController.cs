using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD  pour la gestion des pièges (Traps) d'une salle
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class TrapsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public TrapsController(AppDbContext db) => _db = db;

    /// <summary>Retourne tous les pièges existants </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Trap>>> GetAll()
        => await _db.Traps.Include(t => t.Room).ToListAsync();

    /// <summary>Retourne un piège par son Id </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Trap>> GetById(Guid id)
    {
      var trap = await _db.Traps.FindAsync(id);
      return trap is null ? NotFound() : Ok(trap);
    }

    /// <summary>Crée un nouveau piège </summary>
    [HttpPost]
    public async Task<ActionResult<Trap>> Create([FromBody] Trap trap)
    {
      if (!_db.Rooms.Any(r => r.Id == trap.RoomId))
        return BadRequest("La salle associée n'existe pas.");

      _db.Traps.Add(trap);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = trap.Id }, trap);
    }

    /// <summary>Met à jour un piège existant </summary>
    [HttpPut("{id:guid}")]
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
