using BlazorGame.Client;
using BlazorGame.Client.Services;
using BlazorGame.Client.Services.Admin;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// HttpClient configuré pour pointer vers l'ApiGateway
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5001") // ApiGateway en développement
});

// Services métier
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DungeonService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ScoresService>();
builder.Services.AddScoped<IAdminApiClient, AdminApiClient>();

await builder.Build().RunAsync();
