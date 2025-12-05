using GameService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddScoped<DungeonGeneratorService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Désactiver la redirection HTTPS si demandé (nécessaire pour le container Docker)
var disableHttpsRedirection = Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECTION");
if (!string.Equals(disableHttpsRedirection, "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}
app.MapControllers();
app.Run();

