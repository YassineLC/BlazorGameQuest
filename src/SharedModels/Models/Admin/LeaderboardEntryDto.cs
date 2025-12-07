using System;

namespace SharedModels.Models.Admin;

/// <summary>
/// Ligne du classement administrateur.
/// </summary>
public sealed record LeaderboardEntryDto(
    Guid PlayerId,
    string PlayerName,
    int TotalScore,
    int SessionCount,
    int MaxDepthReached,
    DateTime LastUpdate);
