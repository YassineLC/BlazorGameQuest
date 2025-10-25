using System;
using System.Collections.Generic;

namespace SharedModels.Models
{
  /// <summary>
  /// Niveaux de difficulté possibles pour une salle
  /// </summary>
  public enum RoomDifficulty { Easy, Medium, Hard }

  /// <summary>
  /// Représente une salle d’un donjon avec ses pièges et ses objets
  /// </summary>
  public class Room
  {
    /// <summary>Identifiant unique de la salle</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Position de la salle dans le donjon</summary>
    public int Order { get; set; }

    /// <summary>Niveau de difficulté de la salle</summary>
    public RoomDifficulty Difficulty { get; set; }

    /// <summary>Description courte de la salle</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Points gagnés si la salle est réussie</summary>
    public int PointsReward { get; set; }

    /// <summary>Liste des pièges présents dans la salle</summary>
    public ICollection<Trap> Traps { get; set; } = new List<Trap>();

    /// <summary>Liste des objets disponibles dans la salle</summary>
    public ICollection<Item> Items { get; set; } = new List<Item>();

    /// <summary>clé etrangère vers le donjon auquel appartient la salle</summary>
    public Guid DungeonId { get; set; }

    /// <summary>Référence au donjon parent</summary>
    public Dungeon? Dungeon { get; set; }
  }
}
