using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SharedModels.Models.Admin;

namespace BlazorGame.Client.Services.Admin;

/// <inheritdoc />
public sealed class AdminApiClient : IAdminApiClient
{
    private readonly HttpClient _http;

    public AdminApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<DashboardStatsDto>("api/admin/dashboard-stats", cancellationToken);
    }

    public async Task<IReadOnlyList<PlayerAdminDto>> GetPlayersAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<List<PlayerAdminDto>>("api/admin/players", cancellationToken)
            ?? new List<PlayerAdminDto>();
    }

    public async Task SetPlayerStatusAsync(Guid playerId, bool isActive, CancellationToken cancellationToken = default)
    {
        var endpoint = isActive
            ? $"api/admin/players/{playerId}/enable"
            : $"api/admin/players/{playerId}/disable";

        var response = await _http.PutAsync(endpoint, content: null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<ScoreAdminDto>> GetScoresAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<List<ScoreAdminDto>>("api/admin/scores", cancellationToken)
            ?? new List<ScoreAdminDto>();
    }

    public async Task<ScoresStatsDto?> GetScoresStatsAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<ScoresStatsDto>("api/admin/scores-stats", cancellationToken);
    }

    public async Task<IReadOnlyList<SessionAdminDto>> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<List<SessionAdminDto>>("api/admin/sessions", cancellationToken)
            ?? new List<SessionAdminDto>();
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(string scope, CancellationToken cancellationToken = default)
    {
        scope = string.IsNullOrWhiteSpace(scope) ? "global" : scope;
        return await _http.GetFromJsonAsync<List<LeaderboardEntryDto>>($"api/admin/leaderboard/{scope}", cancellationToken)
            ?? new List<LeaderboardEntryDto>();
    }

    public async Task<IReadOnlyList<DungeonAdminDto>> GetDungeonsAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<List<DungeonAdminDto>>("api/admin/dungeons", cancellationToken)
            ?? new List<DungeonAdminDto>();
    }

    public async Task<ExportFileResult> DownloadExportAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(endpoint, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? $"export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";

        return new ExportFileResult(content, fileName, contentType);
    }
}
