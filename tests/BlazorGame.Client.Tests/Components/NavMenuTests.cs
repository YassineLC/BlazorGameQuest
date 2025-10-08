using Bunit;
using Xunit;
using FluentAssertions;
using BlazorGame.Client.Layout;

namespace BlazorGame.Tests
{
  public class NavMenuTests : TestContext
  {
    [Fact]
    public void TopRow_RendersNavbarBrandAndToggleButton()
    {
      var cut = RenderComponent<NavMenu>();

      var navbarBrand = cut.Find(".navbar-brand");
      navbarBrand.TextContent.Should().Contain("BlazorGameQuest");

      var toggleButton = cut.Find("button.navbar-toggler");
      toggleButton.Should().NotBeNull();
    }

    [Fact]
    public void NavMenu_CollapsedByDefault()
    {
      var cut = RenderComponent<NavMenu>();

      var navDiv = cut.Find("div.nav-scrollable");
      navDiv.ClassList.Should().Contain("collapse"); 
    }

    [Fact]
    public void ToggleNavMenu_ChangesCollapseClass()
    {
      var cut = RenderComponent<NavMenu>();

      
      var navDiv = cut.Find("div.nav-scrollable");
      navDiv.ClassList.Should().Contain("collapse");

      
      var toggleButton = cut.Find("button.navbar-toggler");
      toggleButton.Click();

      navDiv.ClassList.Should().NotContain("collapse");

     
      toggleButton.Click();

      navDiv.ClassList.Should().Contain("collapse");
    }

    [Fact]
    public void NavLinks_RenderExpectedForUnauthenticatedUser()
    {
      var cut = RenderComponent<NavMenu>();

      cut.Find("a.nav-link[href='']").TextContent.Should().Contain("Accueil");
      cut.Find("a.nav-link[href='game/new-adventure']").TextContent.Should().Contain("Nouvelle Aventure");
      cut.Find("a.nav-link[href='leaderboard']").TextContent.Should().Contain("Classements");

      cut.Find("a.nav-link[href='auth/login']").TextContent.Should().Contain("Connexion");
      cut.Find("a.nav-link[href='auth/register']").TextContent.Should().Contain("Inscription");
    }

    [Fact]
    public void ClickingNavMenu_AlsoTogglesCollapse()
    {
      var cut = RenderComponent<NavMenu>();
      var navDiv = cut.Find("div.nav-scrollable");

      navDiv.ClassList.Should().Contain("collapse");

      navDiv.Click();

      navDiv.ClassList.Should().NotContain("collapse");
    }
  }
}
