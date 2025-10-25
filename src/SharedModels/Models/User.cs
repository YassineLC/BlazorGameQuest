using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Models
{
  /// <summary>
  /// Représente le rôle d'un utilisateur dans le système
  /// Un utilisateur peut être un joueur classique ou un administrateur
  /// Ce type énuméré permet d'adapter les droits et les comportements dans l'application
  /// </summary>
  public enum UserRole
  {
    Player,
    Admin
  }

  /// <summary>
  /// Classe principale représentant un utilisateur du jeu
  /// </summary>
  public class User
  {
    /// <summary>
    /// Identifiant unique de l'utilisateur (clé primaire en base de données)
    /// Utilise un GUID pour l'unicité globale entre les microservices
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nom d'utilisateur obligatoire et limité à 50 caractères
    /// </summary>
    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    /// <summary>
    /// Adresse e-mail de l'utilisateur
    /// </summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    /// <summary>
    /// Hash sécurisé du mot de passe
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    /// <summary>
    /// Rôle de l'utilisateur dans le système (Player/Admin)
    /// Par défaut, un nouvel utilisateur est considéré comme un joueur.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Player;

    /// <summary>
    /// Liste des sessions de jeu associées à cet utilisateur
    /// </summary>
    public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    /// <summary>
    /// Historique des scores de l'utilisateur à travers ses différentes parties
    /// </summary>
    public ICollection<Score> Scores { get; set; } = new List<Score>();
  }
}
