using ApiGateway.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD pour la gestion des salles (Rooms)
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Produces("application/json")]
  public class RoomsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public RoomsController(AppDbContext db) => _db = db;

    [HttpGet]
    [SwaggerOperation(
      Summary = "Liste les salles",
      Description = "Retourne l'ensemble des salles avec leurs objets et pièges associés.",
      OperationId = "Rooms_GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Room>))]
    public async Task<ActionResult<IEnumerable<Room>>> GetAll()
        => await _db.Rooms
            .AsNoTracking()
            .Include(r => r.Items)
            .Include(r => r.Traps)
            .ToListAsync();

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
      Summary = "Récupère une salle",
      Description = "Renvoie la salle identifiée avec ses objets et pièges.",
      OperationId = "Rooms_GetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Room))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Room>> GetById(Guid id)
    {
      var room = await _db.Rooms
          .AsNoTracking()
          .Include(r => r.Items)
          .Include(r => r.Traps)
          .FirstOrDefaultAsync(r => r.Id == id);
      return room is null ? NotFound() : Ok(room);
    }

    [HttpPost]
    [SwaggerOperation(
      Summary = "Crée une salle",
      Description = "Ajoute une nouvelle salle à un donjon existant.",
      OperationId = "Rooms_Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Room))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Room>> Create(Room room)
    {
      if (!await _db.Dungeons.AsNoTracking().AnyAsync(d => d.Id == room.DungeonId))
        return BadRequest("Le Donjon associé n'existe pas.");

      _db.Rooms.Add(room);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
      Summary = "Met à jour une salle",
      Description = "Actualise les informations d'une salle existante.",
      OperationId = "Rooms_Update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, Room input)
    {
      if (id != input.Id) return BadRequest();
      var room = await _db.Rooms.FindAsync(id);
      if (room is null) return NotFound();

      room.Description = input.Description;
      room.Order = input.Order;
      room.Difficulty = input.Difficulty;
      room.PointsReward = input.PointsReward;
      room.DungeonId = input.DungeonId;

      await _db.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
      Summary = "Supprime une salle",
      Description = "Efface la salle identifiée.",
      OperationId = "Rooms_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
      var room = await _db.Rooms.FindAsync(id);
      if (room is null) return NotFound();

      _db.Rooms.Remove(room);
      await _db.SaveChangesAsync();
      return NoContent();
    }
  }
}
