using System.Linq;
using ApiGateway.Data;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using SharedModels.Models.Admin;

namespace ApiGateway.Features.Admin;

/// <summary>
/// Fournit les données et actions dédiées à la console d'administration.
/// </summary>
public sealed class AdminDataService : IAdminDataService
{
    private readonly AppDbContext _db;

    public AdminDataService(AppDbContext db) => _db = db;

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalPlayers = await _db.Users.CountAsync(cancellationToken);
        var totalSessions = await _db.GameSessions.CountAsync(cancellationToken);
        var totalScores = await _db.Scores.CountAsync(cancellationToken);
        var totalDungeons = await _db.Dungeons.CountAsync(cancellationToken);
        var averageScore = totalScores > 0
            ? (int)await _db.Scores.AverageAsync(s => s.Value, cancellationToken)
            : 0;

        return new DashboardStatsDto(
            totalPlayers,
            totalSessions,
            totalScores,
            totalDungeons,
            averageScore,
            DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<PlayerAdminDto>> GetPlayersAsync(CancellationToken cancellationToken = default)
    {
        var players = await _db.Users
            .AsNoTracking()
            .Select(u => new { u.Id, u.Username, u.Email, u.Role })
            .ToListAsync(cancellationToken);

        if (players.Count == 0)
        {
            return Array.Empty<PlayerAdminDto>();
        }

        var playerIds = players.Select(p => p.Id).ToArray();

        var sessionCounts = await _db.GameSessions
            .AsNoTracking()
            .Where(gs => playerIds.Contains(gs.PlayerId))
            .GroupBy(gs => gs.PlayerId)
            .Select(group => new { PlayerId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.PlayerId, x => x.Count, cancellationToken);

        var scoreAverages = await _db.Scores
            .AsNoTracking()
            .Where(s => playerIds.Contains(s.PlayerId))
            .GroupBy(s => s.PlayerId)
            .Select(group => new { PlayerId = group.Key, Average = group.Average(s => s.Value) })
            .ToDictionaryAsync(x => x.PlayerId, x => (int)x.Average, cancellationToken);

        var now = DateTime.UtcNow;

        return players
            .Select(player => new PlayerAdminDto(
                player.Id,
                player.Username,
                player.Email,
                sessionCounts.TryGetValue(player.Id, out var sessionCount) ? sessionCount : 0,
                scoreAverages.TryGetValue(player.Id, out var average) ? average : 0,
                player.Role == UserRole.Player,
                now))
            .ToList();
    }

    public async Task SetPlayerActiveStatusAsync(Guid playerId, bool isActive, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == playerId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException($"Utilisateur {playerId} introuvable");
        }

        user.Role = isActive ? UserRole.Player : UserRole.Admin;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScoreAdminDto>> GetScoresAsync(CancellationToken cancellationToken = default)
    {
        var scores = await _db.Scores
            .AsNoTracking()
            .Include(s => s.Player)
            .OrderByDescending(s => s.AchievedAt)
            .Select(s => new
            {
                s.Id,
                PlayerName = s.Player != null ? s.Player.Username : "Unknown",
                s.SessionId,
                s.Value,
                s.AchievedAt
            })
            .ToListAsync(cancellationToken);

        if (scores.Count == 0)
        {
            return Array.Empty<ScoreAdminDto>();
        }

        var sessionLookup = await BuildSessionDungeonLookupAsync(scores.Select(s => s.SessionId), cancellationToken);

        return scores
            .Select(score => new ScoreAdminDto(
                score.Id,
                score.PlayerName,
                score.SessionId is { } sessionId && sessionLookup.TryGetValue(sessionId, out var dungeon)
                    ? dungeon
                    : "Unknown",
                score.Value,
                0,
                0,
                0,
                false,
                score.AchievedAt))
            .ToList();
    }

    public async Task<ScoresStatsDto> GetScoresStatsAsync(CancellationToken cancellationToken = default)
    {
        var aggregate = await _db.Scores
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Average = group.Average(s => s.Value),
                Max = group.Max(s => s.Value),
                Sum = group.Sum(s => s.Value)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return aggregate is null
            ? new ScoresStatsDto(0, 0, 0)
            : new ScoresStatsDto((int)aggregate.Average, aggregate.Max, aggregate.Sum);
    }

    public async Task<IReadOnlyList<SessionAdminDto>> GetSessionsAsync(CancellationToken cancellationToken = default)
    {
        var sessions = await _db.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Dungeon)
            .AsNoTracking()
            .Select(gs => new SessionAdminDto(
                gs.Id,
                gs.Player!.Username,
                gs.Dungeon!.Name,
                0,
                gs.CurrentScore,
                gs.IsFinished,
                gs.StartedAt,
                gs.EndedAt ?? DateTime.UtcNow))
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);

        return sessions;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> GetLeaderboardAsync(string scope, int limit, CancellationToken cancellationToken = default)
    {
        scope = scope.ToLowerInvariant();
        var now = DateTime.UtcNow;
        var scoresQuery = _db.Scores.AsNoTracking();

        scoresQuery = scope switch
        {
            "weekly" => scoresQuery.Where(s => s.AchievedAt >= now.AddDays(-7)),
            "daily" => scoresQuery.Where(s => s.AchievedAt >= now.AddDays(-1)),
            _ => scoresQuery
        };

        var aggregates = await scoresQuery
            .GroupBy(s => s.PlayerId)
            .Select(group => new
            {
                PlayerId = group.Key,
                TotalScore = group.Sum(s => s.Value),
                LastUpdate = group.Max(s => s.AchievedAt)
            })
            .OrderByDescending(x => x.TotalScore)
            .Take(limit)
            .ToListAsync(cancellationToken);

        if (aggregates.Count == 0)
        {
            return Array.Empty<LeaderboardEntryDto>();
        }

        var playerIds = aggregates.Select(a => a.PlayerId).ToArray();

        var players = await _db.Users
            .AsNoTracking()
            .Where(u => playerIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Username })
            .ToListAsync(cancellationToken);

        var sessionCounts = await _db.GameSessions
            .AsNoTracking()
            .Where(gs => playerIds.Contains(gs.PlayerId))
            .GroupBy(gs => gs.PlayerId)
            .Select(group => new { PlayerId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        var playerMap = players.ToDictionary(p => p.Id, p => p.Username);
        var sessionMap = sessionCounts.ToDictionary(s => s.PlayerId, s => s.Count);

        return aggregates
            .Select(aggregate => new LeaderboardEntryDto(
                aggregate.PlayerId,
                playerMap.TryGetValue(aggregate.PlayerId, out var username) ? username : "Unknown",
                aggregate.TotalScore,
                sessionMap.TryGetValue(aggregate.PlayerId, out var count) ? count : 0,
                0,
                aggregate.LastUpdate))
            .ToList();
    }

    public async Task<IReadOnlyList<DungeonAdminDto>> GetDungeonsAsync(CancellationToken cancellationToken = default)
    {
        var dungeons = await _db.Dungeons
            .AsNoTracking()
            .Select(d => new { d.Id, d.Name, d.MaxDepth })
            .ToListAsync(cancellationToken);

        if (dungeons.Count == 0)
        {
            return Array.Empty<DungeonAdminDto>();
        }

        var dungeonIds = dungeons.Select(d => d.Id).ToArray();

        var roomCounts = await _db.Rooms
            .AsNoTracking()
            .Where(r => dungeonIds.Contains(r.DungeonId))
            .GroupBy(r => r.DungeonId)
            .Select(group => new { DungeonId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.DungeonId, x => x.Count, cancellationToken);

        var sessionCounts = await _db.GameSessions
            .AsNoTracking()
            .Where(gs => dungeonIds.Contains(gs.DungeonId))
            .GroupBy(gs => gs.DungeonId)
            .Select(group => new { DungeonId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.DungeonId, x => x.Count, cancellationToken);

        var now = DateTime.UtcNow;

        return dungeons
            .Select(dungeon => new DungeonAdminDto(
                dungeon.Id,
                dungeon.Name,
                dungeon.MaxDepth,
                roomCounts.TryGetValue(dungeon.Id, out var rooms) ? rooms : 0,
                0,
                sessionCounts.TryGetValue(dungeon.Id, out var sessions) ? sessions : 0,
                now))
            .ToList();
    }

    public async Task<IReadOnlyList<User>> ExportPlayersAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Users.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GameSession>> ExportSessionsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.GameSessions
            .Include(gs => gs.Player)
            .Include(gs => gs.Dungeon)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Dungeon>> ExportDungeonsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Dungeons.AsNoTracking().ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<Guid, string>> BuildSessionDungeonLookupAsync(IEnumerable<Guid?> sessionIds, CancellationToken cancellationToken)
    {
        var ids = sessionIds
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var sessions = await _db.GameSessions
            .AsNoTracking()
            .Where(gs => ids.Contains(gs.Id))
            .Join(
                _db.Dungeons.AsNoTracking(),
                session => session.DungeonId,
                dungeon => dungeon.Id,
                (session, dungeon) => new { session.Id, DungeonName = dungeon.Name })
            .ToListAsync(cancellationToken);

        return sessions.ToDictionary(s => s.Id, s => s.DungeonName);
    }
}
