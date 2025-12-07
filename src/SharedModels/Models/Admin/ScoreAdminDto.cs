using System;

namespace SharedModels.Models.Admin;

/// <summary>
/// Détails d'un score côté administration.
/// </summary>
public sealed record ScoreAdminDto(
    Guid Id,
    string PlayerName,
    string DungeonName,
    int FinalScore,
    int KillCount,
    int TreasureCount,
    int TrapAvoidedCount,
    bool IsBossDefeated,
    DateTime AchievedAt);
