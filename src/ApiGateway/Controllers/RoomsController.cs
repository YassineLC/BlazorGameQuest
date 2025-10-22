using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// CRUD pour la gestion des salles (Rooms)
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class RoomsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public RoomsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Room>>> GetAll()
        => await _db.Rooms.Include(r => r.Items).Include(r => r.Traps).ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Room>> GetById(Guid id)
    {
      var room = await _db.Rooms.Include(r => r.Items).Include(r => r.Traps)
          .FirstOrDefaultAsync(r => r.Id == id);
      return room is null ? NotFound() : Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<Room>> Create(Room room)
    {
      if (!_db.Dungeons.Any(d => d.Id == room.DungeonId))
        return BadRequest("Le Donjon associé n'existe pas.");

      _db.Rooms.Add(room);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [HttpPut("{id:guid}")]
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
