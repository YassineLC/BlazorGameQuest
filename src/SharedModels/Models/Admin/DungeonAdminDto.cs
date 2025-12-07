using System;

namespace SharedModels.Models.Admin;

/// <summary>
/// Informations sur un donjon côté administration.
/// </summary>
public sealed record DungeonAdminDto(
    Guid Id,
    string Name,
    int MaxDepth,
    int RoomCount,
    int EventCount,
    int SessionCount,
    DateTime CreatedAt);
