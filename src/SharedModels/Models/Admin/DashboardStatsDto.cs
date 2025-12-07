using System;

namespace SharedModels.Models.Admin;

/// <summary>
/// Statistiques globales affichées dans le tableau de bord administrateur.
/// </summary>
public sealed record DashboardStatsDto(
    int TotalPlayers,
    int TotalSessions,
    int TotalScores,
    int TotalDungeons,
    int AverageScore,
    DateTime LastUpdated);
