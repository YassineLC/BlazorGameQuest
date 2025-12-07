using System;

namespace SharedModels.Models.Admin;

/// <summary>
/// Détails d'une session de jeu affichée dans l'espace administrateur.
/// </summary>
public sealed record SessionAdminDto(
    Guid Id,
    string PlayerName,
    string DungeonName,
    int MaxDepthReached,
    int FinalScore,
    bool IsFinished,
    DateTime StartedAt,
    DateTime FinishedAt);
