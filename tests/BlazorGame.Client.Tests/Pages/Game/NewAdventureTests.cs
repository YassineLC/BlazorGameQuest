using Bunit;
using Xunit;
using BlazorGame.Client.Pages.Game;
using System.Net.Http;
using BlazorGame.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using BlazorGame.Client.Tests.TestHelpers;

namespace BlazorGame.Client.Tests.Pages.Game
{
  public class NewAdventureTests : TestContext
  {
    public NewAdventureTests()
    {
      this.AddDefaultClientServices();
    }

    [Fact]
    public void NewAdventure_RendersTitleAndIntro()
    {
      var cut = RenderComponent<NewAdventure>();

      var h1 = cut.Find("h1");
      Assert.Contains("Nouvelle Aventure", h1.TextContent);

      var lead = cut.Find("p.lead");
      Assert.Contains("Prêt à explorer les mystères des donjons", lead.TextContent);
    }

    [Fact]
    public void NewAdventure_ShowsConfigurationScreen_AfterClick()
    {
      var cut = RenderComponent<NewAdventure>();

      var startButton = cut.Find("button.btn-primary");
      startButton.Click();

      var configHeader = cut.Find("h1.text-center");
      Assert.Contains("Configuration du Donjon", configHeader.TextContent);

      var formLabels = cut.FindAll(".form-label");
      Assert.Contains(formLabels, l => l.TextContent.Contains("Nombre de salles"));
      Assert.Contains(formLabels, l => l.TextContent.Contains("Difficulté"));
    }

    [Fact]
    public void NewAdventure_RendersDungeonPreview()
    {
      var cut = RenderComponent<NewAdventure>();

      cut.Find("button.btn-primary").Click();

      var preview = cut.Find("div.dungeon-preview");
      Assert.NotNull(preview);

      Assert.Contains("Aperçu", preview.TextContent);
      Assert.Contains("Salles", preview.TextContent);
      Assert.Contains("Difficulté", preview.TextContent);
      Assert.Contains("Récompenses estimées", preview.TextContent);

      var rooms = cut.FindAll(".room-preview");
      Assert.True(rooms.Count >= 3);
    }

    [Fact]
    public void NewAdventure_RendersNavigationButtons()
    {
      var cut = RenderComponent<NewAdventure>();

      var dashboardButton = cut.Find("a.btn.btn-secondary");
      Assert.Equal("/player/dashboard", dashboardButton.GetAttribute("href"));
      Assert.Contains("Retour au Tableau de Bord", dashboardButton.TextContent);

      cut.Find("button.btn-primary").Click();

      var launchButton = cut.Find("button.btn-success");
      Assert.Contains("Lancer l'Aventure", launchButton.TextContent);

      var backButton = cut.Find("button.btn.btn-secondary");
      Assert.Contains("Retour", backButton.TextContent);
    }
  }
}
