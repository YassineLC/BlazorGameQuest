using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api/[controller]")]
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
