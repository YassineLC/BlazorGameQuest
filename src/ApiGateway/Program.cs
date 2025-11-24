using System.Text.Json.Serialization;
using ApiGateway.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données en mémoire
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("GameQuestDb"));

builder.Services.AddHttpClient("GameService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:GameService"] ?? "https://localhost:7002");
});

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
});


var app = builder.Build();

// Activer CORS
app.UseCors("AllowBlazorClient");

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
