using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>CRUD pour la gestion des objets (Items) </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class ItemsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public ItemsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetAll()
        => await _db.Items.Include(i => i.Room).ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Item>> GetById(Guid id)
    {
      var item = await _db.Items.FindAsync(id);
      return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Item>> Create(Item item)
    {
      if (!_db.Rooms.Any(r => r.Id == item.RoomId))
        return BadRequest("La salle associée n'existe pas.");

      _db.Items.Add(item);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Item input)
    {
      if (id != input.Id) return BadRequest();
      var item = await _db.Items.FindAsync(id);
      if (item is null) return NotFound();

      item.Name = input.Name;
      item.Description = input.Description;
      item.Value = input.Value;
      item.RoomId = input.RoomId;

      await _db.SaveChangesAsync();
      return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var item = await _db.Items.FindAsync(id);
      if (item is null) return NotFound();

      _db.Items.Remove(item);
      await _db.SaveChangesAsync();
      return NoContent();
    }
  }
}
