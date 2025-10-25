using System;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Models
{
  /// <summary>Objet trouvable dans une salle</summary>
  public class Item
  {
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Value { get; set; }

    /// <summary>clé étrangère vers la salle propriétaire</summary>
    public Guid RoomId { get; set; }

    /// <summary>Navigation vers la salle</summary>
    public Room? Room { get; set; }
  }
}
