using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SharedModels.Models.Admin;

namespace BlazorGame.Client.Services.Admin;

/// <summary>
/// Client HTTP fortement typé pour interagir avec l'API d'administration.
/// </summary>
public interface IAdminApiClient
{
    Task<DashboardStatsDto?> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlayerAdminDto>> GetPlayersAsync(CancellationToken cancellationToken = default);
    Task SetPlayerStatusAsync(Guid playerId, bool isActive, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScoreAdminDto>> GetScoresAsync(CancellationToken cancellationToken = default);
    Task<ScoresStatsDto?> GetScoresStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(string scope, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DungeonAdminDto>> GetDungeonsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionAdminDto>> GetSessionsAsync(CancellationToken cancellationToken = default);
    Task<ExportFileResult> DownloadExportAsync(string endpoint, CancellationToken cancellationToken = default);
}

/// <summary>
/// Encapsule un fichier exporté par l'API d'administration.
/// </summary>
public sealed record ExportFileResult(byte[] Content, string FileName, string ContentType);
