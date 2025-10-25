using ApiGateway.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Choix provider: InMemory pour tests, sinon Postgres
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemory");

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
    /// </summary>
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}

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
