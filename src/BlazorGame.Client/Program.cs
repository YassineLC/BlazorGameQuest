using BlazorGame.Client;
using BlazorGame.Client.Services;
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
builder.Services.AddScoped<DungeonService>();

await builder.Build().RunAsync();
