using SharedModels.Models;

namespace GameService.Services;

/// <summary>
/// Service de génération procédurale de donjons basé sur un graphe
/// </summary>
public class DungeonGeneratorService
{
    private Random _random = new();

    // Générateurs de variations textuelles
    private readonly string[] _adjectives = { "sombre", "ancien", "oublié", "mystérieux", "lugubre", "sinistre", "inquiétant", "étrange", "hanté", "maudit" };
    private readonly string[] _ambiances = { "humide", "glacial", "étouffant", "oppressant", "silencieux", "résonnant", "obscur", "lumineux" };
    private readonly string[] _details = { "des gravats", "des ossements", "des inscriptions", "des symboles", "des traces", "des marques", "des restes", "des débris" };

    /// <summary>
    /// Génère un donjon complet avec un graphe de salles
    /// </summary>
    public DungeonGenerationResult Generate(int seed, int maxDepth = 10, int minBranches = 1, int maxBranches = 3)
    {
        _random = new Random(seed);
        var rooms = new List<Room>();
        var startRoom = CreateRoom(RoomType.Start, 0, "Entrée du Donjon",
          GenerateUniqueDescription(RoomType.Start));
        rooms.Add(startRoom);

        GenerateRecursive(startRoom, rooms, 1, maxDepth, minBranches, maxBranches);

        return new DungeonGenerationResult { Rooms = rooms, Seed = seed };
    }

    private void GenerateRecursive(Room parent, List<Room> all, int depth, int maxDepth, int minB, int maxB)
    {
        if (depth >= maxDepth)
        {
            var boss = CreateRoom(RoomType.Boss, depth, GetName(RoomType.Boss),
              GenerateUniqueDescription(RoomType.Boss));
            all.Add(boss);

            // Utiliser une liste temporaire puis l'assigner
            var nextIds = parent.NextRoomIds; // Récupérer la liste actuelle
            nextIds.Add(boss.Id);
            parent.NextRoomIds = nextIds; // Réassigner pour forcer la sérialisation
            return;
        }

        // Variabilité dans le nombre de branches (plus aléatoire)
        int branches = _random.Next(100) < 70 ? _random.Next(minB, maxB + 1) : _random.Next(1, maxB + 2);

        // Liste temporaire pour les enfants
        var childIds = new List<Guid>();

        for (int i = 0; i < branches; i++)
        {
            var type = DetermineType(depth, maxDepth);
            var child = CreateRoom(type, depth, GetName(type), GenerateUniqueDescription(type));
            all.Add(child);
            childIds.Add(child.Id);

            // Variation dans la profondeur : parfois sauter un niveau
            int nextDepth = _random.Next(100) < 85 ? depth + 1 : depth + 2;
            if (nextDepth < maxDepth)
            {
                GenerateRecursive(child, all, nextDepth, maxDepth, minB, maxB);
            }
            else if (nextDepth == maxDepth)
            {
                var finalBoss = CreateRoom(RoomType.Boss, maxDepth, GetName(RoomType.Boss), GenerateUniqueDescription(RoomType.Boss));
                all.Add(finalBoss);

                // Assigner directement
                child.NextRoomIds = new List<Guid> { finalBoss.Id };
            }
        }

        // Assigner tous les IDs des enfants au parent
        parent.NextRoomIds = childIds;
    }

    private RoomType DetermineType(int depth, int maxDepth)
    {
        var roll = _random.Next(100);

        // Distribution plus variée selon la profondeur
        if (depth <= 2)
        {
            if (roll < 60) return RoomType.Combat;
            if (roll < 80) return RoomType.Event;
            return RoomType.Treasure;
        }
        else if (depth <= maxDepth / 2)
        {
            if (roll < 45) return RoomType.Combat;
            if (roll < 65) return RoomType.Event;
            if (roll < 80) return RoomType.Treasure;
            if (roll < 90) return RoomType.Shop;
            return RoomType.Rest;
        }
        else
        {
            if (roll < 50) return RoomType.Combat;
            if (roll < 70) return RoomType.Event;
            if (roll < 85) return RoomType.Rest;
            return RoomType.Shop;
        }
    }
    private Room CreateRoom(RoomType type, int depth, string name, string desc) => new()
    {
        Id = Guid.NewGuid(),
        Type = type,
        Depth = depth,
        Name = name,
        Description = desc,
        Order = 0,
        PointsReward = type == RoomType.Boss ? 100 : _random.Next(10, 50),
        Difficulty = depth < 4 ? RoomDifficulty.Easy : depth < 7 ? RoomDifficulty.Medium : RoomDifficulty.Hard
    };

    private string GetName(RoomType type)
    {
        var adj = Pick(_adjectives);

        return type switch
        {
            RoomType.Combat => Pick(
              $"Salle des Combats {adj}",
              $"Arène {adj}",
              $"Chambre {adj} des Gardiens",
              $"Corridor {adj}",
              "Salle des Duels",
              "Fosse de Combat",
              "Arène Ensanglantée"
            ),
            RoomType.Treasure => Pick(
              $"Trésor {adj}",
              $"Coffre {adj}",
              $"Cache {adj}",
              "Chambre Secrète",
              "Salle des Richesses",
              "Voûte Précieuse"
            ),
            RoomType.Event => Pick(
              $"Lieu {adj}",
              $"Chambre {adj}",
              $"Sanctuaire {adj}",
              "Salle des Énigmes",
              "Passage Étrange",
              "Hall Mystique"
            ),
            RoomType.Shop => Pick(
              "Échoppe du Marchand",
              "Marché Noir",
              "Campement Itinérant",
              "Refuge du Voyageur",
              "Boutique Mystérieuse"
            ),
            RoomType.Rest => Pick(
              "Sanctuaire Paisible",
              "Campement Sûr",
              "Source Apaisante",
              "Refuge Tranquille",
              "Havre de Paix"
            ),
            RoomType.Boss => Pick(
              "Salle du Boss",
              "Chambre du Gardien",
              "Trône des Ténèbres",
              "Antre du Maître"
            ),
            _ => "Salle Inconnue"
        };
    }

    /// <summary>
    /// Génère une description unique en combinant des éléments aléatoires
    /// </summary>
    private string GenerateUniqueDescription(RoomType type)
    {
        var ambiance = Pick(_ambiances);
        var detail = Pick(_details);
        var adj = Pick(_adjectives);

        return type switch
        {
            RoomType.Start => $"L'entrée du donjon est {ambiance}. {Pick("Des torches vacillent", "L'obscurité règne", "Un vent froid souffle")} et {detail} jonchent le sol.",

            RoomType.Combat => Pick(
              $"Cette salle {ambiance} porte les traces d'un combat {adj}. {Pick("Des armes brisées", "Du sang séché", "Des équipements abandonnés")} témoignent de la violence passée.",
              $"L'atmosphère est {ambiance}. {Pick("Des grognements", "Des bruits inquiétants", "Des cris lointains")} résonnent depuis les ombres.",
              $"Un lieu {adj} et {ambiance}. {detail} parsèment le sol, et {Pick("des créatures", "quelque chose", "une présence")} rôde dans les ténèbres.",
              $"Les murs sont marqués par {detail}. L'air {ambiance} annonce un combat imminent.",
              $"Cette pièce {ambiance} vibre encore de l'énergie d'affrontements passés. {Pick("Méfiez-vous", "Restez vigilant", "Préparez-vous")}."
            ),

            RoomType.Treasure => Pick(
              $"Un endroit {adj} où {Pick("l'or scintille", "des gemmes brillent", "des richesses s'accumulent")}. L'atmosphère est {ambiance} mais prometteuse.",
              $"Des {detail} précieux reposent ici depuis longtemps. {Pick("Un coffre", "Des reliques", "Des trésors")} attendent dans ce lieu {ambiance}.",
              $"La salle est {ambiance} mais remplie de {Pick("richesses oubliées", "trésors anciens", "merveilles perdues")}. {detail} indiquent un passé glorieux.",
              $"Un trésor {adj} dans un endroit {ambiance}. {Pick("L'éclat de l'or", "Les pierres précieuses", "Les objets de valeur")} illuminent faiblement l'obscurité."
            ),

            RoomType.Event => Pick(
              $"Cette salle {ambiance} dégage une aura {adj}. {detail} forment des motifs étranges sur les murs.",
              $"Un lieu {adj} où {Pick("la magie", "le mystère", "l'inexplicable")} imprègne l'air {ambiance}. {detail} brillent faiblement.",
              $"L'atmosphère {ambiance} de cette pièce est troublante. {Pick("Des runes", "Des symboles", "Des inscriptions")} couvrent les surfaces.",
              $"Un endroit {adj} empli de {Pick("mystère", "magie ancienne", "pouvoir oublié")}. Le lieu est {ambiance} et chargé d'énergie.",
              $"Quelque chose d'{adj} réside ici. L'air {ambiance} vibre et {detail} suggèrent une présence ancienne."
            ),

            RoomType.Shop => Pick(
              $"Un {Pick("marchand", "voyageur", "commerçant")} {adj} s'est installé dans ce lieu {ambiance}. {Pick("Des marchandises rares", "Des objets précieux", "Des curiosités")} sont exposés.",
              $"Une échoppe {ambiance} remplie de {Pick("merveilles", "objets étranges", "trouvailles rares")}. Le {Pick("marchand", "vendeur", "négociant")} vous observe.",
              $"Ce lieu {adj} sert de {Pick("marché", "boutique", "comptoir")}. L'atmosphère {ambiance} n'empêche pas le commerce.",
              $"Un {Pick("refuge", "campement", "poste")} commercial dans cet endroit {ambiance}. {Pick("Un voyageur", "Un marchand", "Un négociant")} propose ses services."
            ),

            RoomType.Rest => Pick(
              $"Un {Pick("sanctuaire", "havre", "refuge")} {ambiance} mais sûr. {Pick("Une source", "Un feu de camp", "Des provisions")} permet de reprendre des forces.",
              $"Ce lieu {adj} offre un répit bienvenu. L'atmosphère {ambiance} est paradoxalement {Pick("apaisante", "réconfortante", "rassurante")}.",
              $"Un endroit {ambiance} où vous pouvez vous {Pick("reposer", "restaurer", "ressourcer")}. {detail} indiquent que d'autres sont passés ici.",
              $"Une pièce {ambiance} mais {Pick("paisible", "tranquille", "sereine")}. {Pick("La fatigue", "L'épuisement", "La tension")} se dissipe ici."
            ),

            RoomType.Boss => Pick(
              $"Une salle {adj} d'une taille {Pick("immense", "colossale", "monumentale")}. L'atmosphère {ambiance} annonce un affrontement décisif.",
              $"Le cœur {adj} du donjon. L'air {ambiance} vibre d'une puissance {Pick("terrible", "redoutable", "écrasante")}.",
              $"Un lieu {ambiance} où règne une présence {adj}. {detail} témoignent de son pouvoir terrifiant.",
              $"La {Pick("chambre", "salle", "arène")} finale, {ambiance} et {adj}. {Pick("Le maître", "Le gardien", "Le seigneur")} de ce lieu vous attend."
            ),

            _ => $"Une salle {adj} du donjon. L'atmosphère est {ambiance}."
        };
    }

    private string Pick(params string[] options) => options[_random.Next(options.Length)];
}

public class DungeonGenerationResult
{
    public List<Room> Rooms { get; set; } = new();
    public int Seed { get; set; }
}
