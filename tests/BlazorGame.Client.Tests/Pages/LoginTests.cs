using System.Threading.Tasks;
using Bunit;
using Xunit;
using Microsoft.AspNetCore.Components.Forms;
using BlazorGame.Client.Pages;

namespace BlazorGame.Client.Tests.Pages
{
  public class LoginTests : TestContext
  {
    [Fact]
    public void Login_RendersFormElements()
    {
      var cut = RenderComponent<Login>();

      var h2 = cut.Find("h2");
      Assert.Equal("Connexion", h2.TextContent.Trim());

      var emailInput = cut.Find("#email");
      Assert.Equal("votre@email.com", emailInput.GetAttribute("placeholder"));

      var passwordInput = cut.Find("#password");
      Assert.Equal("password", passwordInput.GetAttribute("type"));

      var submitButton = cut.Find("button[type='submit']");
      Assert.Contains("Se Connecter", submitButton.TextContent);

      var registerLink = cut.Find("a[href='/auth/register']");
      Assert.Contains("S'inscrire", registerLink.TextContent);
    }

    [Fact]
    public void Login_SubmitForm_ShowsSpinner()
    {
      var cut = RenderComponent<Login>();

      // Simule un remplissage de formulaire
      cut.Find("#email").Change("test@example.com");
      cut.Find("#password").Change("123456");

      cut.Find("form").Submit();

      var submitButton = cut.Find("button[type='submit']");
      Assert.True(submitButton.HasAttribute("disabled"));

      var spinner = cut.Find("span.spinner-border");
      Assert.NotNull(spinner);
    }
  }
}