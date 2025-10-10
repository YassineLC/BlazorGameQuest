using System;
using Bunit;
using Xunit;
using BlazorGame.Client.Pages.Game;

namespace BlazorGame.Client.Tests.Pages.Game
{
  public class NewAdventureTests : TestContext
  {
    [Fact]
    public void NewAdventure_RendersTitleAndIntro()
    {
      var cut = RenderComponent<NewAdventure>();

      var h1 = cut.Find("h1");
      Assert.Contains("Nouvelle Aventure", h1.TextContent);

      var lead = cut.Find("p.lead");
      Assert.Contains("Préparez-vous à explorer un donjon mystérieux", lead.TextContent);
    }

    [Fact]
    public void NewAdventure_RendersDungeonPreview()
    {
      var cut = RenderComponent<NewAdventure>();

      var previewCard = cut.Find("div.preview-card");
      Assert.NotNull(previewCard);

      Assert.Contains("Nombre de salles :", previewCard.TextContent);
      Assert.Contains("Difficulté :", previewCard.TextContent);
      Assert.Contains("Système de récompenses :", previewCard.TextContent);
      Assert.Contains("Mécaniques de jeu :", previewCard.TextContent);
      Assert.Contains("Types d'ennemis :", previewCard.TextContent);
      Assert.Contains("Objets et équipements :", previewCard.TextContent);

      var alert = cut.Find("div.alert-info");
      Assert.Contains("La logique de jeu complète sera implémentée", alert.TextContent);
    }

    [Fact]
    public void NewAdventure_RendersNavigationButtons()
    {
      var cut = RenderComponent<NewAdventure>();

      var enterButton = cut.Find("a[href='/game/room/1']");
      Assert.NotNull(enterButton);
      Assert.Contains("Entrer dans le Donjon", enterButton.TextContent);

      var dashboardButton = cut.Find("a[href='/player/dashboard']");
      Assert.NotNull(dashboardButton);
      Assert.Contains("Retour au Tableau de Bord", dashboardButton.TextContent);
    }
  }
}
