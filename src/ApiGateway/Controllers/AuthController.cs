using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IHttpClientFactory httpFactory, ILogger<AuthController> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Authentifie un utilisateur",
        Description = "Proxifie l'appel de connexion vers le service d'authentification et retourne la réponse brute de Keycloak.",
        OperationId = "Auth_Login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(JsonElement))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] JsonElement loginRequest)
    {
        try
        {
            var client = _httpFactory.CreateClient("AuthService");
            // Note: The path here must match the route in AuthenticationServices
            var response = await client.PostAsJsonAsync("api/auth/login", loginRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AuthService login failed: {StatusCode} - {Content}", response.StatusCode, content);
                return StatusCode((int)response.StatusCode, content);
            }

            return Ok(JsonDocument.Parse(content).RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying login request");
            return StatusCode(500, "Internal server error");
        }
    }
}
