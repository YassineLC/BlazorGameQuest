
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiGateway.Data;
using ApiGateway.Features.Admin;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données en mémoire
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("GameQuestDb"));

builder.Services.AddHttpClient("GameService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:GameService"] ?? "https://localhost:7002");
});

builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AuthService"] ?? "http://localhost:5003");
});

builder.Services.AddScoped<IAdminDataService, AdminDataService>();

var keycloakAuthority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/blazorgamequest";
var keycloakIssuer = builder.Configuration["Keycloak:ValidIssuer"] ?? keycloakAuthority;
var keycloakJwksUri = builder.Configuration["Keycloak:JwksUri"] ?? $"{keycloakAuthority.TrimEnd('/')}/protocol/openid-connect/certs";
var keycloakJwksCache = new KeycloakJwksCache(keycloakJwksUri);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidIssuer = keycloakIssuer,
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity)
                    return Task.CompletedTask;

                var registeredRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                static void AppendRolesFromElement(JsonElement element, ClaimsIdentity identity, ISet<string> registeredRoles)
                {
                    if (element.ValueKind != JsonValueKind.Array)
                        return;

                    foreach (var roleJson in element.EnumerateArray())
                    {
                        var role = roleJson.GetString();
                        if (string.IsNullOrWhiteSpace(role) || !registeredRoles.Add(role))
                            continue;

                        identity.AddClaim(new Claim(identity.RoleClaimType, role));
                    }
                }

                void ExtractRolesFromClaim(string claimType)
                {
                    var claim = context.Principal!.FindFirst(claimType);
                    if (claim is null)
                        return;

                    using var document = JsonDocument.Parse(claim.Value);

                    if (claimType == "resource_access")
                    {
                        foreach (var client in document.RootElement.EnumerateObject())
                        {
                            if (client.Value.TryGetProperty("roles", out var clientRoles))
                            {
                                AppendRolesFromElement(clientRoles, identity, registeredRoles);
                            }
                        }
                        return;
                    }

                    if (document.RootElement.TryGetProperty("roles", out var rolesElement))
                    {
                        AppendRolesFromElement(rolesElement, identity, registeredRoles);
                    }
                }

                ExtractRolesFromClaim("realm_access");
                ExtractRolesFromClaim("resource_access");

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = async context =>
            {
                if (context.Exception is SecurityTokenSignatureKeyNotFoundException)
                {
                    await keycloakJwksCache.RefreshSigningKeysAsync().ConfigureAwait(false);
                }
            }
        };

        options.TokenValidationParameters.IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
        {
            var keys = keycloakJwksCache.GetSigningKeysAsync().GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(kid) && keys.All(k => !string.Equals(k.KeyId, kid, StringComparison.Ordinal)))
            {
                keys = keycloakJwksCache.RefreshSigningKeysAsync().GetAwaiter().GetResult();
            }

            return keys;
        };
    });

builder.Services.AddAuthorization();

// Configuration CORS pour permettre au client Blazor d'appeler l'API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blazor Game Quest API",
        Version = "v1",
        Description = "Projet académique : micro-services de gestion du jeu Blazor.\n" +
                      "Réalisé par Louisa MAIBECHE et Yassine LAHMAR-CHERIF , étudiants en ingénierie logicielle.",
        Contact = new OpenApiContact
        {
            Name = "Louisa",
            Email = "louisa.maibeche@efrei.net"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });

    options.EnableAnnotations();
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blazor Game Quest API v2");
        c.DocumentTitle = "Blazor Game Quest - Swagger UI";
    });
}

// Désactiver la redirection HTTPS si demandé (nécessaire pour le container Docker)
var disableHttpsRedirection = Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECTION");
if (!string.Equals(disableHttpsRedirection, "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

app.UseRouting();

// Activer CORS
app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

internal sealed class KeycloakJwksCache
{
    private readonly string _jwksUri;
    private readonly HttpClient _httpClient = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IEnumerable<SecurityKey>? _cachedKeys;
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RetryThrottle = TimeSpan.FromSeconds(5);

    public KeycloakJwksCache(string jwksUri)
    {
        _jwksUri = jwksUri;
    }

    public Task<IEnumerable<SecurityKey>> GetSigningKeysAsync() => GetSigningKeysInternalAsync(forceRefresh: false);

    public Task<IEnumerable<SecurityKey>> RefreshSigningKeysAsync() => GetSigningKeysInternalAsync(forceRefresh: true);

    private async Task<IEnumerable<SecurityKey>> GetSigningKeysInternalAsync(bool forceRefresh)
    {
        if (!forceRefresh && _cachedKeys is not null && DateTimeOffset.UtcNow - _lastRefresh < CacheDuration)
        {
            return _cachedKeys;
        }

        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!forceRefresh && _cachedKeys is not null && DateTimeOffset.UtcNow - _lastRefresh < CacheDuration)
            {
                return _cachedKeys;
            }

            if (forceRefresh && DateTimeOffset.UtcNow - _lastRefresh < RetryThrottle && _cachedKeys is not null)
            {
                return _cachedKeys;
            }

            var response = await _httpClient.GetStringAsync(_jwksUri).ConfigureAwait(false);
            var keys = new JsonWebKeySet(response).GetSigningKeys().ToArray();
            _cachedKeys = keys;
            _lastRefresh = DateTimeOffset.UtcNow;
            return keys;
        }
        finally
        {
            _lock.Release();
        }
    }
}
