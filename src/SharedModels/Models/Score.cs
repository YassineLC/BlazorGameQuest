using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedModels.Models
{
  /// <summary>
  /// Représente le score obtenu par un joueur lors d'une partie
  /// </summary>
  public class Score
  {
    /// <summary>Identifiant unique du score</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>clé etrangère vers le joueur ayant obtenu ce score</summary>
    public Guid PlayerId { get; set; }

    /// <summary>Référence au joueur associé</summary>
    public User? Player { get; set; }

    /// <summary>Valeur du score atteint</summary>
    public int Value { get; set; }

    /// <summary>Date et heure d’obtention du score</summary>
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;

    /// <summary>clé etrangère optionnelle vers la session de jeu correspondante</summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// Nom du donjon associé à la session (propriété non persistée, utile pour l'affichage)
    /// </summary>
    [NotMapped]
    public string? DungeonName { get; set; }
  }
}
