using System.Net.Http.Json;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

/// <summary>
/// Service côté client pour gérer la navigation dans les donjons.
/// Communique UNIQUEMENT avec l'ApiGateway (pattern microservices).
/// </summary>
public class DungeonService
{
    private readonly HttpClient _http;
    private readonly string _apiBaseUrl = "/api"; // Relatif, géré par le reverse proxy

    private Dungeon? _currentDungeon;
    private Room? _currentRoom;

    public DungeonService(HttpClient http)
    {
        _http = http;
    }

    public Dungeon? CurrentDungeon => _currentDungeon;
    public Room? CurrentRoom => _currentRoom;

    /// <summary>
    /// Crée un nouveau donjon avec génération procédurale via l'ApiGateway
    /// </summary>
    public async Task<Dungeon?> CreateDungeonAsync(string name, int maxDepth = 5, int? seed = null)
    {
        var request = new
        {
            Name = name,
            MaxDepth = maxDepth,
            Seed = seed
        };

        var response = await _http.PostAsJsonAsync($"{_apiBaseUrl}/dungeons", request);

        if (response.IsSuccessStatusCode)
        {
            _currentDungeon = await response.Content.ReadFromJsonAsync<Dungeon>();

            // Charger les salles complètes
            if (_currentDungeon != null)
            {
                await LoadDungeonWithRoomsAsync(_currentDungeon.Id);
            }

            return _currentDungeon;
        }

        return null;
    }

    /// <summary>
    /// Récupère un donjon complet avec toutes ses salles
    /// </summary>
    public async Task<Dungeon?> LoadDungeonWithRoomsAsync(Guid dungeonId)
    {
        _currentDungeon = await _http.GetFromJsonAsync<Dungeon>($"{_apiBaseUrl}/dungeons/{dungeonId}");

        if (_currentDungeon?.Rooms != null && _currentDungeon.Rooms.Count > 0)
        {
            // Définir la salle de départ
            _currentRoom = _currentDungeon.Rooms.FirstOrDefault(r => r.Type == RoomType.Start);
        }

        return _currentDungeon;
    }

    /// <summary>
    /// Liste tous les donjons disponibles
    /// </summary>
    public async Task<List<Dungeon>> GetAllDungeonsAsync()
    {
        return await _http.GetFromJsonAsync<List<Dungeon>>($"{_apiBaseUrl}/dungeons") ?? new List<Dungeon>();
    }

    /// <summary>
    /// Navigue vers une salle spécifique
    /// </summary>
    public Room? NavigateToRoom(Guid roomId)
    {
        if (_currentDungeon?.Rooms == null) return null;

        _currentRoom = _currentDungeon.Rooms.FirstOrDefault(r => r.Id == roomId);
        return _currentRoom;
    }

    /// <summary>
    /// Récupère les salles suivantes accessibles depuis la salle actuelle
    /// </summary>
    public List<Room> GetNextRooms()
    {
        if (_currentRoom == null || _currentDungeon?.Rooms == null)
            return new List<Room>();

        var nextRooms = new List<Room>();

        // Debug: vérifier les IDs des salles suivantes
        Console.WriteLine($"CurrentRoom: {_currentRoom.Name} (ID: {_currentRoom.Id})");
        Console.WriteLine($"NextRoomIdsJson: {_currentRoom.NextRoomIdsJson}");
        Console.WriteLine($"NextRoomIds count: {_currentRoom.NextRoomIds.Count}");

        foreach (var nextRoomId in _currentRoom.NextRoomIds)
        {
            Console.WriteLine($"Looking for room with ID: {nextRoomId}");
            var room = _currentDungeon.Rooms.FirstOrDefault(r => r.Id == nextRoomId);
            if (room != null)
            {
                Console.WriteLine($"Found room: {room.Name}");
                nextRooms.Add(room);
            }
            else
            {
                Console.WriteLine($"Room not found!");
            }
        }

        Console.WriteLine($"Total next rooms found: {nextRooms.Count}");
        return nextRooms;
    }

    /// <summary>
    /// Réinitialise le donjon actuel
    /// </summary>
    public void ResetCurrentDungeon()
    {
        _currentDungeon = null;
        _currentRoom = null;
    }
}
