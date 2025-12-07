namespace SharedModels.Models.Admin;

/// <summary>
/// Statistiques agrégées sur les scores.
/// </summary>
public sealed record ScoresStatsDto(
    int AverageScore,
    int MaxScore,
    int TotalScores);
