using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorGame.Client.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HttpClient _httpClient;

    public CustomAuthStateProvider(IJSRuntime jsRuntime, HttpClient httpClient)
    {
        _jsRuntime = jsRuntime;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void NotifyUserAuthentication(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
        {
            return Array.Empty<Claim>();
        }

        var segments = jwt.Split('.');
        if (segments.Length < 2)
        {
            return Array.Empty<Claim>();
        }

        var payload = segments[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes)
            ?? new Dictionary<string, object>();

        var claims = keyValuePairs
            .Select(kvp => new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty))
            .ToList();
        var registeredRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void AppendRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role) || !registeredRoles.Add(role))
            {
                return;
            }

            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Mapping spécifique pour Keycloak
        // Si "preferred_username" existe, on l'ajoute comme ClaimTypes.Name pour que User.Identity.Name fonctionne
        if (keyValuePairs.TryGetValue("preferred_username", out var username))
        {
            claims.Add(new Claim(ClaimTypes.Name, username?.ToString() ?? string.Empty));
        }
        // Fallback sur "name" si preferred_username n'est pas là
        else if (keyValuePairs.TryGetValue("name", out var name))
        {
            claims.Add(new Claim(ClaimTypes.Name, name?.ToString() ?? string.Empty));
        }

        // Gestion des rôles (realm_access.roles ou resource_access.client.roles)
        // Keycloak met souvent les rôles dans une structure JSON complexe
        if (keyValuePairs.TryGetValue("realm_access", out var realmAccessObj))
        {
            try
            {
                if (TryParseJsonElement(realmAccessObj, out var realmAccess) &&
                    realmAccess.TryGetProperty("roles", out var rolesElement))
                {
                    foreach (var roleElement in rolesElement.EnumerateArray())
                    {
                        AppendRole(roleElement.GetString());
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs de parsing des rôles
            }
        }

        if (keyValuePairs.TryGetValue("resource_access", out var resourceAccessObj))
        {
            try
            {
                if (TryParseJsonElement(resourceAccessObj, out var resourceAccess))
                {
                    foreach (var client in resourceAccess.EnumerateObject())
                    {
                        if (!client.Value.TryGetProperty("roles", out var clientRoles))
                        {
                            continue;
                        }

                        foreach (var roleElement in clientRoles.EnumerateArray())
                        {
                            AppendRole(roleElement.GetString());
                        }
                    }
                }
            }
            catch
            {
                // Ignorer les erreurs de parsing des rôles client
            }
        }

        return claims;
    }

    private static bool TryParseJsonElement(object? source, out JsonElement element)
    {
        switch (source)
        {
            case JsonElement jsonElement:
                element = jsonElement;
                return true;
            case string json when !string.IsNullOrWhiteSpace(json):
                element = JsonSerializer.Deserialize<JsonElement>(json);
                return true;
            default:
                var raw = source?.ToString();
                if (string.IsNullOrWhiteSpace(raw))
                {
                    element = default;
                    return false;
                }

                element = JsonSerializer.Deserialize<JsonElement>(raw);
                return true;
        }
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
