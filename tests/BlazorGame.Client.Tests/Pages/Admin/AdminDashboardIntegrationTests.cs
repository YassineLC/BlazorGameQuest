using System;
using Bunit;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using BlazorGame.Client.Pages.Admin;

namespace BlazorGame.Client.Tests.Pages.Admin
{
  /// <summary>
  /// Integration tests for admin dashboard pages
  /// Tests that pages render correctly and have expected basic structure
  /// </summary>
  public class AdminDashboardIntegrationTests : TestContext
  {
    public AdminDashboardIntegrationTests()
    {
      var httpClient = new System.Net.Http.HttpClient();
      Services.AddScoped(sp => httpClient);
    }

    [Fact]
    public void AdminDashboard_RendersSuccessfully()
    {
      var cut = RenderComponent<Dashboard>();
      Assert.NotNull(cut);
      Assert.True(cut.Markup.Length > 0);
    }

    [Fact]
    public void AdminDashboard_RendersStatisticCards()
    {
      var cut = RenderComponent<Dashboard>();
      
      var cards = cut.FindAll(".card-medieval");
      Assert.Equal(4, cards.Count);
    }

    [Fact]
    public void AdminDashboard_RendersQuickAccessButtons()
    {
      var cut = RenderComponent<Dashboard>();
      
      var buttons = cut.FindAll(".btn-group a");
      Assert.NotEmpty(buttons);
    }

    [Fact]
    public void AdminPlayers_RendersSuccessfully()
    {
      var cut = RenderComponent<Players>();
      Assert.NotNull(cut);
      Assert.True(cut.Markup.Length > 0);
    }

    [Fact]
    public void AdminPlayers_RendersPageTitle()
    {
      var cut = RenderComponent<Players>();
      var heading = cut.Find("h1");
      Assert.NotNull(heading);
    }

    [Fact]
    public void AdminScores_RendersSuccessfully()
    {
      var cut = RenderComponent<Scores>();
      Assert.NotNull(cut);
      Assert.True(cut.Markup.Length > 0);
    }

    [Fact]
    public void AdminScores_RendersPageTitle()
    {
      var cut = RenderComponent<Scores>();
      var heading = cut.Find("h1");
      Assert.NotNull(heading);
    }

    [Fact]
    public void AdminSessions_RendersSuccessfully()
    {
      var cut = RenderComponent<Sessions>();
      Assert.NotNull(cut);
      Assert.True(cut.Markup.Length > 0);
    }

    [Fact]
    public void AdminSessions_RendersPageTitle()
    {
      var cut = RenderComponent<Sessions>();
      var heading = cut.Find("h1");
      Assert.NotNull(heading);
    }

    [Fact]
    public void AdminLeaderboard_RendersSuccessfully()
    {
      var cut = RenderComponent<Leaderboard>();
      Assert.NotNull(cut);
      Assert.True(cut.Markup.Length > 0);
    }

    [Fact]
    public void AdminLeaderboard_RendersPageTitle()
    {
      var cut = RenderComponent<Leaderboard>();
      var heading = cut.Find("h1");
      Assert.NotNull(heading);
    }

    [Fact]
    public void AdminLeaderboard_RendersTabs()
    {
      var cut = RenderComponent<Leaderboard>();
      var buttons = cut.FindAll("button");
      Assert.NotEmpty(buttons);
    }

    [Fact]
    public void AdminDungeons_RendersSuccessfully()
    {
      var cut = RenderComponent<Dungeons>();
      Assert.NotNull(cut);
      Assert.True(cut.Markup.Length > 0);
    }

    [Fact]
    public void AdminDungeons_RendersPageTitle()
    {
      var cut = RenderComponent<Dungeons>();
      var heading = cut.Find("h1");
      Assert.NotNull(heading);
    }

    [Fact]
    public void AdminExport_RendersSuccessfully()
    {
      var cut = RenderComponent<Export>();
      Assert.NotNull(cut);
      Assert.True(cut.Markup.Length > 0);
    }

    [Fact]
    public void AdminExport_RendersPageTitle()
    {
      var cut = RenderComponent<Export>();
      var heading = cut.Find("h1");
      Assert.NotNull(heading);
      Assert.Contains("Exporter", heading.TextContent);
    }

    [Fact]
    public void AdminExport_RendersExportButtons()
    {
      var cut = RenderComponent<Export>();
      var buttons = cut.FindAll("button");
      Assert.True(buttons.Count >= 4); 
    }

    [Fact]
    public void AdminPages_ContainNavigationLinks()
    {
      var cut = RenderComponent<Dashboard>();
      var links = cut.FindAll("a");
      
      var hrefs = string.Concat(links.Select(a => a.GetAttribute("href") ?? ""));
      
      Assert.Contains("/admin/", hrefs);
    }
  }
}
