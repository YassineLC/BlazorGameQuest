using Bunit;
using Xunit;
using BlazorGame.Client.Pages;

namespace BlazorGame.Client.Tests.Pages
{
  public class ComingSoonTests : TestContext
  {
    [Fact]
    public void ComingSoon_RendersMainContent()
    {
      var cut = RenderComponent<ComingSoon>();

      Assert.Equal("Fonctionnalité À Venir", cut.Find("h2").TextContent.Trim());

      Assert.Contains("Cette fonctionnalité est en cours de développement",
          cut.Find("p.lead").TextContent);
    }

    [Fact]
    public void ComingSoon_RendersFeaturesList()
    {
      var cut = RenderComponent<ComingSoon>();

      var listItems = cut.FindAll(".features-preview ul li");
      Assert.Equal(5, listItems.Count);

      Assert.Equal("Système de jeu complet", listItems[0].TextContent.Trim());
      Assert.Equal("Exploration de donjons", listItems[1].TextContent.Trim());
      Assert.Equal("Combats tactiques", listItems[2].TextContent.Trim());
      Assert.Equal("Système de scores", listItems[3].TextContent.Trim());
      Assert.Equal("Progression du joueur", listItems[4].TextContent.Trim());
    }

    [Fact]
    public void ComingSoon_RendersNavigationButtons()
    {
      var cut = RenderComponent<ComingSoon>();

      var dashboardButton = cut.Find("a.btn-primary");
      Assert.Equal("/player/dashboard", dashboardButton.GetAttribute("href"));
      Assert.Contains("Tableau de Bord", dashboardButton.TextContent);

      var homeButton = cut.Find("a.btn-outline-secondary");
      Assert.Equal("/", homeButton.GetAttribute("href"));
      Assert.Contains("Accueil", homeButton.TextContent);
    }

    [Fact]
    public void ComingSoon_RendersFooterText()
    {
      var cut = RenderComponent<ComingSoon>();

      var footer = cut.Find("small.text-muted");
      Assert.Contains("Restez connecté pour découvrir les nouvelles fonctionnalités",
          footer.TextContent);
    }
  }
}
