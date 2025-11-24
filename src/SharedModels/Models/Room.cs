using System;
using System.Collections.Generic;

namespace SharedModels.Models
{
  /// <summary>
  /// Niveaux de difficulté possibles pour une salle
  /// </summary>
  public enum RoomDifficulty { Easy, Medium, Hard }

  /// <summary>
  /// Types de salles dans le donjon (pour génération procédurale)
  /// </summary>
  public enum RoomType { Start, Combat, Treasure, Event, Shop, Rest, Boss }

  /// <summary>
  /// Représente une salle d'un donjon avec ses pièges et ses objets
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

    // === Propriétés pour génération procédurale ===

    /// <summary>Nom de la salle</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Profondeur dans le graphe du donjon</summary>
    public int Depth { get; set; }

    /// <summary>Type de salle (pour logique de jeu)</summary>
    public RoomType Type { get; set; }

    /// <summary>IDs des salles suivantes (stocké en JSON)</summary>
    public string NextRoomIdsJson { get; set; } = "[]";

    /// <summary>Helper pour manipuler les NextRoomIds</summary>
    public List<Guid> NextRoomIds
    {
      get => System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(NextRoomIdsJson) ?? new();
      set => NextRoomIdsJson = System.Text.Json.JsonSerializer.Serialize(value);
    }
  }
}
