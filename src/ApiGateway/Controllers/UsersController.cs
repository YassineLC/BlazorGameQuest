using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// Endpoints CRUD pour gérer les utilisateurs
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  public class UsersController : ControllerBase
  {
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    /// <summary>Retourne la liste complète des utilisateurs </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll()
        => await _db.Users.AsNoTracking().ToListAsync();

    /// <summary>Retourne un utilisateur par son Id </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> GetById(Guid id)
    {
      var user = await _db.Users.FindAsync(id);
      return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Crée un utilisateur </summary>
    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
      if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
        return BadRequest("Username et Email sont requis.");

      _db.Users.Add(user);
      await _db.SaveChangesAsync();
      return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>Met à jour un utilisateur </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] User input)
    {
      if (id != input.Id) return BadRequest("Id route ≠ Id body.");
      var user = await _db.Users.FindAsync(id);
      if (user is null) return NotFound();

      user.Username = input.Username;
      user.Email = input.Email;
      user.PasswordHash = input.PasswordHash;
      user.Role = input.Role;

      await _db.SaveChangesAsync();
      return NoContent();
    }

    /// <summary>Supprime un utilisateur </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
      var user = await _db.Users.FindAsync(id);
      if (user is null) return NotFound();

      _db.Users.Remove(user);
      await _db.SaveChangesAsync();
      return NoContent();
    }

    /// <summary>Synchronise ou crée un utilisateur depuis Keycloak</summary>
    [HttpPost("sync")]
    public async Task<ActionResult<User>> SyncUser([FromBody] SyncUserRequest request)
    {
      if (string.IsNullOrWhiteSpace(request.Id) || string.IsNullOrWhiteSpace(request.Username))
        return BadRequest("Id et Username sont requis.");

      if (!Guid.TryParse(request.Id, out var userId))
        return BadRequest("Id doit être un GUID valide.");

      // Chercher si l'utilisateur existe déjà
      var existingUser = await _db.Users.FindAsync(userId);

      if (existingUser != null)
      {
        // Mettre à jour les informations si nécessaire
        existingUser.Username = request.Username;
        if (!string.IsNullOrEmpty(request.Email))
          existingUser.Email = request.Email;

        await _db.SaveChangesAsync();
        return Ok(existingUser);
      }

      // Créer un nouvel utilisateur
      var newUser = new User
      {
        Id = userId,
        Username = request.Username,
        Email = request.Email ?? $"{request.Username}@keycloak.local",
        PasswordHash = "KEYCLOAK_MANAGED", // Le mot de passe est géré par Keycloak
        Role = UserRole.Player
      };

      _db.Users.Add(newUser);
      await _db.SaveChangesAsync();

      return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
    }
  }

  public record SyncUserRequest(string Id, string Username, string? Email);
}
