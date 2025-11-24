using System.Net.Http.Json;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class GameService
{
    private readonly HttpClient _http;
    private readonly string _apiBase = "/api/play";

    public Guid? CurrentSessionId { get; private set; }

    public GameService(HttpClient http)
    {
        _http = http;
    }

    public async Task<GameSession?> StartSessionAsync(Guid dungeonId, Guid? playerId = null)
    {
        var req = new { PlayerId = playerId, DungeonId = dungeonId };
        var resp = await _http.PostAsJsonAsync($"{_apiBase}/start", req);
        if (!resp.IsSuccessStatusCode) return null;

        var session = await resp.Content.ReadFromJsonAsync<GameSession>();
        if (session != null) CurrentSessionId = session.Id;
        return session;
    }

    public async Task<VisitResult?> VisitRoomAsync(Guid sessionId, Guid roomId, string? choice = null)
    {
        var req = new { SessionId = sessionId, RoomId = roomId, Choice = choice };
        var resp = await _http.PostAsJsonAsync($"{_apiBase}/visit", req);
        if (!resp.IsSuccessStatusCode) return null;
        return await resp.Content.ReadFromJsonAsync<VisitResult>();
    }

    public async Task<bool> EndSessionAsync(Guid sessionId)
    {
        var req = new { SessionId = sessionId };
        var resp = await _http.PostAsJsonAsync($"{_apiBase}/end", req);
        return resp.IsSuccessStatusCode;
    }
}

public class VisitResult
{
    public int NewScore { get; set; }
    public bool IsFinished { get; set; }
    public string? Message { get; set; }
    public List<Guid> NextRoomIds { get; set; } = new();
}
