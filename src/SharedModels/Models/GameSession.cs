using System;
using System.Collections.Generic;
using SharedModels.Models;

/// <summary>
/// Représente une session de jeu d’un joueur dans un donjon
/// </summary>
public class GameSession
{
  /// <summary>Identifiant unique de la session </summary>
  public Guid Id { get; set; } = Guid.NewGuid();

  /// <summary>FK vers le joueur participant à la session </summary>
  public Guid PlayerId { get; set; }

  /// <summary>Référence au joueur concerné </summary>
  public User? Player { get; set; }

  /// <summary>FK vers le donjon joué pendant la session </summary>
  public Guid DungeonId { get; set; }

  /// <summary>Référence au donjon associé </summary>
  public Dungeon? Dungeon { get; set; }

  /// <summary>Date et heure de début de la session </summary>
  public DateTime StartedAt { get; set; } = DateTime.UtcNow;

  /// <summary>Date et heure de fin de la session (si terminée) </summary>
  public DateTime? EndedAt { get; set; }

  /// <summary>Score actuel du joueur pendant la session </summary>
  public int CurrentScore { get; set; }

  /// <summary>Indique si la session est terminée </summary>
  public bool IsFinished { get; set; }
}
