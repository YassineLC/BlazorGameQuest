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

      var heading = cut.Find("h2");
      Assert.Contains("Fonctionnalité À Venir", heading.TextContent);

      var paragraph = cut.Find("p.lead");
      Assert.Contains("Cette fonctionnalité est en cours de développement", paragraph.TextContent);
    }

    [Fact]
    public void ComingSoon_RendersFeaturesList()
    {
      var cut = RenderComponent<ComingSoon>();

      var listItems = cut.FindAll(".features-preview ul li");
      Assert.Equal(5, listItems.Count);

      Assert.Contains("Système de jeu complet", listItems[0].TextContent);
      Assert.Contains("Exploration de donjons", listItems[1].TextContent);
      Assert.Contains("Combats tactiques", listItems[2].TextContent);
      Assert.Contains("Système de scores", listItems[3].TextContent);
      Assert.Contains("Progression du joueur", listItems[4].TextContent);
    }

    [Fact]
    public void ComingSoon_RendersNavigationButtons()
    {
      var cut = RenderComponent<ComingSoon>();

      var dashboardButton = cut.Find("a.btn-primary");
      Assert.Equal("/player/dashboard", dashboardButton.GetAttribute("href"));
      Assert.Contains("Tableau de Bord", dashboardButton.TextContent);

      var homeButton = cut.Find("a.btn-secondary");
      Assert.Equal("/", homeButton.GetAttribute("href"));
      Assert.Contains("Accueil", homeButton.TextContent);
    }

    [Fact]
    public void ComingSoon_RendersFooterText()
    {
      var cut = RenderComponent<ComingSoon>();

      var footer = cut.Find("small");
      Assert.Contains("Restez connecté pour découvrir les nouvelles fonctionnalités", footer.TextContent);
    }
  }
}
