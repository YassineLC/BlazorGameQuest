using System;

namespace SharedModels.Models
{
  /// <summary>
  /// Types possibles de pièges présents dans les salles
  /// </summary>
  public enum TrapType { Spike, Poison, Explosion, Snare, Fire }

  /// <summary>
  /// Représente un piège dans une salle de donjon
  /// </summary>
  public class Trap
  {
    /// <summary>Identifiant unique du piège</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Type de piège </summary>
    public TrapType Type { get; set; }

    /// <summary>Probabilité que le piège se déclenche (en %)</summary>
    public int ChanceToTriggerPercent { get; set; } = 50;

    /// <summary>Dégâts infligés si le piège est déclenché</summary>
    public int Damage { get; set; } = 10;

    /// <summary>Indique si le piège peut être désamorcé</summary>
    public bool CanBeDisarmed { get; set; } = true;

    /// <summary>Indique si le piège a été désamorcé</summary>
    public bool IsDisarmed { get; set; } = false;

    /// <summary>clé etrangère vers la salle à laquelle appartient le piège</summary>
    public Guid RoomId { get; set; }

    /// <summary>Référence à la salle propriétaire du piège</summary>
    public Room? Room { get; set; }
  }
}
