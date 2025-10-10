using Bunit;
using Xunit;
using BlazorGame.Client.Pages;

namespace BlazorGame.Client.Tests.Pages
{
  public class HomeTests : TestContext
  {

    [Fact]
    public void Home_RendersHeroSection_ForUnauthenticatedUser()
    {
      var cut = RenderComponent<Home>();

      var h1 = cut.Find("h1.display-4");
      Assert.Equal("BlazorGameQuest", h1.TextContent.Trim());

      var leadText = cut.Find("p.lead");
      Assert.Contains("Incarnez un aventurier et explorez des donjons générés aléatoirement", leadText.TextContent);

      // Vérifie les boutons pour non connecté
      var loginButton = cut.Find("a.btn-primary[href='/auth/login']");
      Assert.Contains("Se connecter", loginButton.TextContent);

      var registerButton = cut.Find("a.btn-primary[href='/auth/register']");
      Assert.Contains("S'inscrire", registerButton.TextContent);
    }

    [Fact]
    public void Home_RendersFeaturesSection()
    {
      var cut = RenderComponent<Home>();

      var features = cut.FindAll(".feature-card");
      Assert.Equal(3, features.Count);

      Assert.Contains("Donjons Aléatoires", features[0].TextContent);
      Assert.Contains("Choix Stratégiques", features[1].TextContent);
      Assert.Contains("Classements", features[2].TextContent);
    }

    [Fact]
    public void Home_RendersRulesSection()
    {
      var cut = RenderComponent<Home>();

      var rulesTitle = cut.Find(".rules-section h2.text-center");
      Assert.Contains("Règles du Jeu", rulesTitle.TextContent);

      var rulesItems = cut.FindAll(".rules-section ul.list-group li.list-group-item");
      Assert.Equal(5, rulesItems.Count);

      Assert.Contains("Objectif", rulesItems[0].TextContent);
      Assert.Contains("Choix disponibles", rulesItems[1].TextContent);
      Assert.Contains("Risques", rulesItems[2].TextContent);
      Assert.Contains("Récompenses", rulesItems[3].TextContent);
      Assert.Contains("Fin de partie", rulesItems[4].TextContent);
    }
  }
}
