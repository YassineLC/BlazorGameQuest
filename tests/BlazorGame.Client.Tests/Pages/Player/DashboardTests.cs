using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using Xunit;
using BlazorGame.Client.Pages.Player;

namespace BlazorGame.Client.Tests.Pages.Player
{
  public class DashboardTests : TestContext
  {
    [Fact]
    public void Dashboard_RendersWelcomeTitle()
    {
      var cut = RenderComponent<Dashboard>();

      
      var title = cut.Find("h1");
      Assert.Contains("Bienvenue", title.TextContent);
      Assert.Contains("Aventurier", title.TextContent);
    }

    [Fact]
    public void Dashboard_RendersNewAdventureButton()
    {
      var cut = RenderComponent<Dashboard>();

      var button = cut.Find("a[href='/game/new-adventure']");
      Assert.NotNull(button);
      Assert.Contains("Commencer l'Aventure", button.TextContent);
    }

    [Fact]
    public void Dashboard_RendersTopPlayersSection()
    {
      var cut = RenderComponent<Dashboard>();

      var leaderboard = cut.Find("div.leaderboard-card");
      Assert.NotNull(leaderboard);

      Assert.Contains("Classement à venir", leaderboard.TextContent);
    }

    [Fact]
    public void Dashboard_RendersRecentGamesSection_WhenNoGames()
    {
      var cut = RenderComponent<Dashboard>();

      var recentGamesSection = cut.Find("div.stats-card");
      Assert.NotNull(recentGamesSection);

      // Message par défaut si recentGames vide
      Assert.Contains("Aucune partie jouée pour le moment", recentGamesSection.TextContent);
    }
  }
}
