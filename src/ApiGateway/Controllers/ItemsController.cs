using ApiGateway.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiGateway.Controllers
{
  /// <summary>CRUD pour la gestion des objets (Items) </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Produces("application/json")]
  public class ItemsController : ControllerBase
  {
    private readonly AppDbContext _db;
    public ItemsController(AppDbContext db) => _db = db;

    [HttpGet]
    [SwaggerOperation(
      Summary = "Liste les objets",
      Description = "Retourne la collection complète des objets avec leur salle associée.",
      OperationId = "Items_GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Item>))]
    public async Task<ActionResult<IEnumerable<Item>>> GetAll()
        => await _db.Items
            .AsNoTracking()
            .Include(i => i.Room)
            .ToListAsync();

    [HttpGet("{id:guid}")]
    [SwaggerOperation(
      Summary = "Récupère un objet",
      Description = "Renvoie l'objet correspondant à l'identifiant fourni.",
      OperationId = "Items_GetById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Item))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Item>> GetById(Guid id)
    {
      var item = await _db.Items
          .AsNoTracking()
          .FirstOrDefaultAsync(i => i.Id == id);
      return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    [SwaggerOperation(
      Summary = "Crée un objet",
      Description = "Ajoute un nouvel objet en vérifiant l'existence de la salle cible.",
      OperationId = "Items_Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Item))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Item>> Create(Item item)
    {
      if (!await _db.Rooms.AsNoTracking().AnyAsync(r => r.Id == item.RoomId))
        return BadRequest("La salle associée n'existe pas.");

      _db.Items.Add(item);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    [SwaggerOperation(
      Summary = "Met à jour un objet",
      Description = "Actualise les informations d'un objet existant.",
      OperationId = "Items_Update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    [SwaggerOperation(
      Summary = "Supprime un objet",
      Description = "Supprime l'objet correspondant à l'identifiant fourni.",
      OperationId = "Items_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
