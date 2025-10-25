using Microsoft.EntityFrameworkCore;
using SharedModels.Models;

namespace ApiGateway.Data
{
    /// <summary>
    /// DbContext centralisant l'accès aux entités du domaine et la configuration EF Core
    /// Les options (provider + connection string) sont injectées par le conteneur DI
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Reçoit la configuration EF Core (provider/connexion) définie dans Program.cs
        /// </summary>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        /// <summary>
        /// Constructeur sans paramètre utilisé uniquement par EF Core au design-time (migrations)
        /// </summary>
        public AppDbContext() { }

        /// <summary>Utilisateurs (rôle Player/Admin via l'enum UserRole) </summary>
        public DbSet<User> Users => Set<User>();
        /// <summary>Donjons </summary>
        public DbSet<Dungeon> Dungeons => Set<Dungeon>();
        /// <summary>Salles d'un donjon </summary>
        public DbSet<Room> Rooms => Set<Room>();
        /// <summary>Pièges d'une salle </summary>
        public DbSet<Trap> Traps => Set<Trap>();
        /// <summary>Objets d'une salle </summary>
        public DbSet<Item> Items => Set<Item>();
        /// <summary>Sessions de jeu d'un joueur dans un donjon </summary>
        public DbSet<GameSession> GameSessions => Set<GameSession>();
        /// <summary>Scores d'un joueur, éventuellement rattachés à une session </summary>
        public DbSet<Score> Scores => Set<Score>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                /// <summary>
                /// Connexion utilisée par les commandes dotnet-ef au design-time
                /// Utilise les variables d'environnement si disponibles
                /// </summary>
                var connectionString = GetConnectionStringFromEnvironment() ??
                    "Host=localhost;Port=5433;Database=gamequest;Username=postgres;Password=admin";

                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        /// <summary>
        /// Construit la chaîne de connexion à partir des variables d'environnement
        /// </summary>
        private static string? GetConnectionStringFromEnvironment()
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST");
            var port = Environment.GetEnvironmentVariable("DB_PORT");
            var database = Environment.GetEnvironmentVariable("DB_DATABASE");
            var username = Environment.GetEnvironmentVariable("DB_USERNAME");
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) ||
                string.IsNullOrEmpty(database) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                return null;
            }

            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        /// <summary>
        /// Configuration explicite des relations et contraintes nécessaires au modèle
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User (1) ---- (N) GameSession
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.Player)
                .WithMany(u => u.GameSessions)
                .HasForeignKey(gs => gs.PlayerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User (1) ---- (N) Score
            modelBuilder.Entity<Score>()
                .HasOne(s => s.Player)
                .WithMany(u => u.Scores)
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Dungeon (1) ---- (N) Room
            modelBuilder.Entity<Room>()
                .HasOne(r => r.Dungeon)
                .WithMany(d => d.Rooms)
                .HasForeignKey(r => r.DungeonId)
                .OnDelete(DeleteBehavior.Cascade);

            // Room: Order unique par Donjon
            modelBuilder.Entity<Room>()
                .HasIndex(r => new { r.DungeonId, r.Order })
                .IsUnique();


            // Room (1) ---- (N) Trap
            modelBuilder.Entity<Trap>()
                .HasOne(t => t.Room)
                .WithMany(r => r.Traps)
                .HasForeignKey(t => t.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Room (1) ---- (N) Item
            modelBuilder.Entity<Item>()
             .HasOne(i => i.Room)
             .WithMany(r => r.Items)
             .HasForeignKey(i => i.RoomId)
             .OnDelete(DeleteBehavior.Cascade);



            // GameSession (N) ---- (1) Dungeon
            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.Dungeon)
                .WithMany()
                .HasForeignKey(gs => gs.DungeonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Score (N) ---- (0..1) GameSession
            modelBuilder.Entity<Score>()
                .HasOne<GameSession>()
                .WithMany()
                .HasForeignKey(s => s.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
