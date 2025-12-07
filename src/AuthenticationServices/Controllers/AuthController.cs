using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, HttpClient httpClient, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Log pour débogage
            _logger.LogInformation("Tentative de connexion pour l'utilisateur: {Username}", request.Username);

            var keycloakUrl = _configuration["Keycloak:TokenUrl"];
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            if (string.IsNullOrEmpty(keycloakUrl))
            {
                return StatusCode(500, "Keycloak TokenUrl is not configured.");
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId ?? "blazorgameclient"),
                new KeyValuePair<string, string>("client_secret", clientSecret ?? ""),
                new KeyValuePair<string, string>("username", request.Username),
                new KeyValuePair<string, string>("password", request.Password),
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var response = await _httpClient.PostAsync(keycloakUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Keycloak login failed: {StatusCode} - {Content}", response.StatusCode, responseContent);
                // Retourner le message d'erreur de Keycloak pour aider au débogage
                return StatusCode((int)response.StatusCode, responseContent);
            }

            return Ok(JsonDocument.Parse(responseContent).RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "Internal server error");
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
