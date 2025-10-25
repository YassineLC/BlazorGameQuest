using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>CRUD pour les Donjons </summary>
  [ApiController, Route("api/[controller]")]
  public class DungeonsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public DungeonsController(AppDbContext db) => _db = db;

    [HttpGet]
    public Task<List<Dungeon>> GetAll() => _db.Dungeons.AsNoTracking().ToListAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Dungeon>> GetById(Guid id)
        => await _db.Dungeons.FindAsync(id) is { } d ? Ok(d) : NotFound();

    [HttpPost]
    public async Task<ActionResult<Dungeon>> Create(Dungeon d)
    {
      if (string.IsNullOrWhiteSpace(d.Name)) return BadRequest("Name requis.");
      _db.Dungeons.Add(d); await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = d.Id }, d);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Dungeon input)
    {
      if (id != input.Id) return BadRequest();
      var d = await _db.Dungeons.FindAsync(id); if (d is null) return NotFound();
      d.Name = input.Name; d.CreatedAt = input.CreatedAt;
      await _db.SaveChangesAsync(); return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var d = await _db.Dungeons.FindAsync(id); if (d is null) return NotFound();
      _db.Dungeons.Remove(d); await _db.SaveChangesAsync(); return NoContent();
    }
  }
}
