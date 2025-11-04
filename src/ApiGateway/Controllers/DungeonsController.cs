using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>CRUD pour les Donjons avec génération procédurale</summary>
  [ApiController, Route("api/[controller]")]
  public class DungeonsController : ControllerBase
  {
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpFactory;

    public DungeonsController(AppDbContext db, IHttpClientFactory httpFactory)
    {
      _db = db;
      _httpFactory = httpFactory;
    }

    /// <summary>Liste les donjons (sans les salles pour alléger)</summary>
    [HttpGet]
    public Task<List<Dungeon>> GetAll() => _db.Dungeons.AsNoTracking().ToListAsync();

    /// <summary>Récupère un donjon avec toutes ses salles</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Dungeon>> GetById(Guid id)
        => await _db.Dungeons.Include(d => d.Rooms).FirstOrDefaultAsync(d => d.Id == id) is { } d ? Ok(d) : NotFound();

    /// <summary>Crée un nouveau donjon avec génération procédurale des salles</summary>
    [HttpPost]
    public async Task<ActionResult<Dungeon>> Create(CreateDungeonRequest req)
    {
      if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name requis.");

      // Appeler GameService pour générer les salles
      var client = _httpFactory.CreateClient("GameService");
      var response = await client.PostAsJsonAsync("/api/dungeongenerator/generate", new
      {
        Seed = req.Seed,
        MaxDepth = req.MaxDepth ?? 10,
        MinBranches = 1,
        MaxBranches = 3
      });

      if (!response.IsSuccessStatusCode) return StatusCode(500, "Erreur génération");

      var result = await response.Content.ReadFromJsonAsync<DungeonGenerationResult>();
      if (result?.Rooms == null) return StatusCode(500, "Résultat invalide");

      // Créer le donjon
      var dungeon = new Dungeon
      {
        Id = Guid.NewGuid(),
        Name = req.Name,
        Seed = result.Seed,
        MaxDepth = req.MaxDepth ?? 10,
        CreatedAt = DateTime.UtcNow
      };

      _db.Dungeons.Add(dungeon);

      // Ajouter les salles générées
      int order = 0;
      foreach (var room in result.Rooms)
      {
        room.DungeonId = dungeon.Id;
        room.Order = order++;
        _db.Rooms.Add(room);
      }

      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = dungeon.Id }, dungeon);
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

  public record CreateDungeonRequest(string Name, int? Seed = null, int? MaxDepth = null);

  public record DungeonGenerationResult(List<Room> Rooms, int Seed);
}

