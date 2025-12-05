using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorGame.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IJSRuntime _jsRuntime;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authStateProvider, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
        _jsRuntime = jsRuntime;
    }

    public async Task<LoginResult> LoginAsync(string username, string password)
    {
        try
        {
            // Call the ApiGateway which proxies to AuthenticationServices
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                if (result.TryGetProperty("access_token", out var tokenProp))
                {
                    var token = tokenProp.GetString();
                    if (!string.IsNullOrEmpty(token))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                        ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(token);

                        // Synchroniser l'utilisateur avec la base de données
                        await SyncUserWithDatabase(token);

                        return new LoginResult { Success = true };
                    }
                }
            }

            return new LoginResult { Success = false, ErrorMessage = "Échec de la connexion. Vérifiez vos identifiants." };
        }
        catch (Exception ex)
        {
            return new LoginResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private async Task SyncUserWithDatabase(string token)
    {
        try
        {
            // Parser le JWT pour extraire les informations de l'utilisateur
            var payload = token.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            if (claims == null) return;

            string? userId = null;
            string? username = null;
            string? email = null;

            if (claims.TryGetValue("sub", out var subElement))
                userId = subElement.GetString();

            if (claims.TryGetValue("preferred_username", out var usernameElement))
                username = usernameElement.GetString();
            else if (claims.TryGetValue("name", out var nameElement))
                username = nameElement.GetString();

            if (claims.TryGetValue("email", out var emailElement))
                email = emailElement.GetString();

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(username))
            {
                // Appeler l'endpoint de synchronisation
                await _httpClient.PostAsJsonAsync("api/users/sync", new { Id = userId, Username = username, Email = email });
            }
        }
        catch
        {
            // Ignorer les erreurs de synchronisation pour ne pas bloquer la connexion
        }
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
