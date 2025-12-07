using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiGateway.Features.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Models.Admin;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiGateway.Controllers
{
  /// <summary>
  /// Endpoints admin pour la gestion du tableau de bord.
  /// </summary>
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "admin,Admin")]
  [Produces("application/json")]
  public class AdminController : ControllerBase
  {
    private readonly IAdminDataService _adminDataService;

    public AdminController(IAdminDataService adminDataService)
    {
      _adminDataService = adminDataService;
    }

    /// <summary>Retourne les statistiques globales du tableau de bord</summary>
    [HttpGet("dashboard-stats")]
    [SwaggerOperation(
      Summary = "Statistiques globales",
      Description = "Retourne les indicateurs clés affichés sur le tableau de bord administrateur.",
      OperationId = "Admin_GetDashboardStats")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DashboardStatsDto))]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats(CancellationToken cancellationToken)
    {
      var stats = await _adminDataService.GetDashboardStatsAsync(cancellationToken);
      return Ok(stats);
    }

    /// <summary>Retourne la liste des joueurs avec statistiques</summary>
    [HttpGet("players")]
    [SwaggerOperation(
      Summary = "Liste les joueurs",
      Description = "Récupère la liste des joueurs avec leurs indicateurs (sessions, scores).",
      OperationId = "Admin_GetPlayers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PlayerAdminDto>))]
    public async Task<ActionResult<IEnumerable<PlayerAdminDto>>> GetPlayers(CancellationToken cancellationToken)
    {
      var players = await _adminDataService.GetPlayersAsync(cancellationToken);
      return Ok(players);
    }

    /// <summary>Désactive un joueur</summary>
    [HttpPut("players/{playerId:guid}/disable")]
    [SwaggerOperation(
      Summary = "Désactive un joueur",
      Description = "Met à jour le statut actif d'un joueur pour l'empêcher de se connecter.",
      OperationId = "Admin_DisablePlayer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DisablePlayer(Guid playerId, CancellationToken cancellationToken)
    {
      await _adminDataService.SetPlayerActiveStatusAsync(playerId, isActive: false, cancellationToken);
      return NoContent();
    }

    /// <summary>Réactive un joueur</summary>
    [HttpPut("players/{playerId:guid}/enable")]
    [SwaggerOperation(
      Summary = "Réactive un joueur",
      Description = "Rend à nouveau actif un joueur précédemment désactivé.",
      OperationId = "Admin_EnablePlayer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EnablePlayer(Guid playerId, CancellationToken cancellationToken)
    {
      await _adminDataService.SetPlayerActiveStatusAsync(playerId, isActive: true, cancellationToken);
      return NoContent();
    }

    /// <summary>Retourne les scores avec détails</summary>
    [HttpGet("scores")]
    [SwaggerOperation(
      Summary = "Liste les scores",
      Description = "Retourne les scores enrichis d'informations joueur et donjon.",
      OperationId = "Admin_GetScores")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ScoreAdminDto>))]
    public async Task<ActionResult<IEnumerable<ScoreAdminDto>>> GetScores(CancellationToken cancellationToken)
    {
      var scores = await _adminDataService.GetScoresAsync(cancellationToken);
      return Ok(scores);
    }

    /// <summary>Retourne les statistiques des scores</summary>
    [HttpGet("scores-stats")]
    [SwaggerOperation(
      Summary = "Statistiques des scores",
      Description = "Calcule les statistiques agrégées (moyenne, max, total) sur l'ensemble des scores.",
      OperationId = "Admin_GetScoresStats")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ScoresStatsDto))]
    public async Task<ActionResult<ScoresStatsDto>> GetScoresStats(CancellationToken cancellationToken)
    {
      var stats = await _adminDataService.GetScoresStatsAsync(cancellationToken);
      return Ok(stats);
    }

    /// <summary>Retourne les sessions de jeu</summary>
    [HttpGet("sessions")]
    [SwaggerOperation(
      Summary = "Liste les sessions",
      Description = "Retourne les sessions de jeu avec le joueur et le donjon associés.",
      OperationId = "Admin_GetSessions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SessionAdminDto>))]
    public async Task<ActionResult<IEnumerable<SessionAdminDto>>> GetSessions(CancellationToken cancellationToken)
    {
      var sessions = await _adminDataService.GetSessionsAsync(cancellationToken);
      return Ok(sessions);
    }

    /// <summary>Retourne les classements</summary>
    [HttpGet("leaderboard/{scope:alpha}")]
    [SwaggerOperation(
      Summary = "Consulte le classement",
      Description = "Retourne un classement des joueurs pour la période demandée (daily, weekly, global).",
      OperationId = "Admin_GetLeaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LeaderboardEntryDto>))]
    public async Task<ActionResult<IEnumerable<LeaderboardEntryDto>>> GetLeaderboard(string scope, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
      var leaderboard = await _adminDataService.GetLeaderboardAsync(scope, limit, cancellationToken);
      return Ok(leaderboard);
    }

    /// <summary>Retourne les donjons avec statistiques</summary>
    [HttpGet("dungeons")]
    [SwaggerOperation(
      Summary = "Liste les donjons",
      Description = "Retourne les donjons agrémentés de KPIs (nombre de salles, sessions).",
      OperationId = "Admin_GetDungeons")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DungeonAdminDto>))]
    public async Task<ActionResult<IEnumerable<DungeonAdminDto>>> GetDungeons(CancellationToken cancellationToken)
    {
      var dungeons = await _adminDataService.GetDungeonsAsync(cancellationToken);
      return Ok(dungeons);
    }

    /// <summary>Exporte tous les joueurs en JSON</summary>
    [HttpGet("export/players")]
    [SwaggerOperation(
      Summary = "Export des joueurs",
      Description = "Génère un fichier JSON des joueurs pour archivage.",
      OperationId = "Admin_ExportPlayers")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<IActionResult> ExportPlayers(CancellationToken cancellationToken)
    {
      var players = await _adminDataService.ExportPlayersAsync(cancellationToken);
      return CreateJsonFile(players, "players");
    }

    /// <summary>Exporte toutes les sessions en JSON</summary>
    [HttpGet("export/sessions")]
    [SwaggerOperation(
      Summary = "Export des sessions",
      Description = "Génère un fichier JSON contenant les sessions de jeu.",
      OperationId = "Admin_ExportSessions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<IActionResult> ExportSessions(CancellationToken cancellationToken)
    {
      var sessions = await _adminDataService.ExportSessionsAsync(cancellationToken);
      return CreateJsonFile(sessions, "sessions");
    }

    /// <summary>Exporte les donjons en JSON</summary>
    [HttpGet("export/dungeons")]
    [SwaggerOperation(
      Summary = "Export des donjons",
      Description = "Génère un fichier JSON des donjons persistés.",
      OperationId = "Admin_ExportDungeons")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    public async Task<IActionResult> ExportDungeons(CancellationToken cancellationToken)
    {
      var dungeons = await _adminDataService.ExportDungeonsAsync(cancellationToken);
      return CreateJsonFile(dungeons, "dungeons");
    }

    private static FileContentResult CreateJsonFile<T>(IEnumerable<T> payload, string prefix)
    {
      var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
      {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      });

      var fileName = $"{prefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
      var bytes = Encoding.UTF8.GetBytes(json);

      return new FileContentResult(bytes, "application/json")
      {
        FileDownloadName = fileName
      };
    }
  }
}
