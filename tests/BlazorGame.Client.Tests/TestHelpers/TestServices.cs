using System;
using System.Net.Http;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using BlazorGame.Client.Services;

namespace BlazorGame.Client.Tests.TestHelpers
{
    public static class TestServices
    {
        public static void AddDefaultClientServices(this TestContext context)
        {
            var http = new HttpClient() { BaseAddress = new Uri("http://localhost") };

            context.Services.AddSingleton(new ToastService());
            context.Services.AddSingleton(new DungeonService(http));
            context.Services.AddSingleton(new GameService(http));
            context.Services.AddSingleton(new ScoresService(http));
        }
    }
}
