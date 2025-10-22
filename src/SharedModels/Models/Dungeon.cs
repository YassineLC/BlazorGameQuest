using System;
using System.Collections.Generic;

namespace SharedModels.Models
{
  /// <summary>
  /// Représente un donjon contenant plusieurs salles
  /// </summary>
  public class Dungeon
  {
    /// <summary>Identifiant unique du donjon</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nom du donjon</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Date de création du donjon</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Liste des salles appartenant à ce donjon</summary>
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
  }
}
