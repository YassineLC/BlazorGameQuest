using System.Net.Http.Json;
using SharedModels.Models;

namespace BlazorGame.Client.Services;

public class ScoresService
{
    private readonly HttpClient _http;
    private readonly string _api = "/api/scores";

    public ScoresService(HttpClient http) => _http = http;

    public async Task<List<Score>> GetAllAsync()
    {
        var list = await _http.GetFromJsonAsync<List<Score>>(_api);
        return list ?? new List<Score>();
    }
}
