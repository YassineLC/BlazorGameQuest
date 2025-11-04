using System.Text.Json.Serialization;
using ApiGateway.Data;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// Charger le fichier .env depuis la racine du projet
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Choix provider: InMemory pour tests, sinon Postgres
var useInMemory = Environment.GetEnvironmentVariable("USE_IN_MEMORY")?.ToLower() == "true"
                 || builder.Configuration.GetValue<bool>("UseInMemory");

if (useInMemory)
{
    /// <summary>
    /// Provider InMemory: pratique pour tester sans DB
    /// </summary>
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseInMemoryDatabase("GameQuestDb"));
}
else
{
    /// <summary>
    /// Provider PostgreSQL: base réelle pour persistance
    /// Utilise les variables d'environnement si disponibles, sinon les appsettings
    /// </summary>
    var connectionString = BuildConnectionString() ??
                          builder.Configuration.GetConnectionString("Postgres");

    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(connectionString));
}

// Fonction pour construire la chaîne de connexion à partir des variables d'environnement
string? BuildConnectionString()
{
    var host = Environment.GetEnvironmentVariable("DB_HOST");
    var port = Environment.GetEnvironmentVariable("DB_PORT");
    var database = Environment.GetEnvironmentVariable("DB_DATABASE");
    var username = Environment.GetEnvironmentVariable("DB_USERNAME");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

    if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) ||
        string.IsNullOrEmpty(database) || string.IsNullOrEmpty(username) ||
        string.IsNullOrEmpty(password))
    {
        return null;
    }

    return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
}

builder.Services.AddHttpClient("GameService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:GameService"] ?? "https://localhost:7002");
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

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
