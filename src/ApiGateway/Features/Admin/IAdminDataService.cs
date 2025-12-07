using SharedModels.Models;
using SharedModels.Models.Admin;

namespace ApiGateway.Features.Admin;

/// <summary>
/// Fournit les données nécessaires aux écrans administrateur.
/// </summary>
public interface IAdminDataService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlayerAdminDto>> GetPlayersAsync(CancellationToken cancellationToken = default);
    Task SetPlayerActiveStatusAsync(Guid playerId, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScoreAdminDto>> GetScoresAsync(CancellationToken cancellationToken = default);
    Task<ScoresStatsDto> GetScoresStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionAdminDto>> GetSessionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(string scope, int limit, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DungeonAdminDto>> GetDungeonsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> ExportPlayersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameSession>> ExportSessionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Dungeon>> ExportDungeonsAsync(CancellationToken cancellationToken = default);
}
