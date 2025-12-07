using System;

namespace SharedModels.Models.Admin;

/// <summary>
/// Informations synthétiques sur un joueur pour l'administration.
/// </summary>
public sealed record PlayerAdminDto(
    Guid Id,
    string Username,
    string Email,
    int SessionCount,
    int AverageScore,
    bool IsActive,
    DateTime CreatedAt);
