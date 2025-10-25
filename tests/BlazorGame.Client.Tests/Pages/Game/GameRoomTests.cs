using Bunit;
using Xunit;
using BlazorGame.Client.Pages.Game;

namespace BlazorGame.Client.Tests.Pages.Game
{
  public class RoomTests : TestContext
  {
    [Fact]
    public void Room_RendersRoomTitle_WithCorrectId()
    {
      int testRoomId = 3;

      var cut = RenderComponent<GameRoom>(parameters => parameters.Add(p => p.RoomId, testRoomId));

      var title = cut.Find("h2.room-title");
      Assert.Contains($"Salle {testRoomId}", title.TextContent);
      Assert.Contains("Crypte Mystérieuse", title.TextContent);
    }

    [Fact]
    public void Room_RendersStoryText()
    {
      var cut = RenderComponent<GameRoom>(parameters => parameters.Add(p => p.RoomId, 1));

      var story = cut.Find("div.story-content-large");
      Assert.Contains("Vous pénétrez dans une salle sombre", story.TextContent);
      Assert.Contains("coffre mystérieux", story.TextContent);
    }

    [Fact]
    public void Room_RendersThreeChoices()
    {
      var cut = RenderComponent<GameRoom>(parameters => parameters.Add(p => p.RoomId, 1));

      var choices = cut.FindAll("button.choice-btn-compact");
      Assert.Equal(3, choices.Count);

      Assert.Contains("Combattre", choices[0].TextContent);
      Assert.Contains("Explorer", choices[1].TextContent);
      Assert.Contains("Fuir", choices[2].TextContent);
    }

    [Fact]
    public void Room_RendersStatusBar()
    {
      var cut = RenderComponent<GameRoom>(parameters => parameters.Add(p => p.RoomId, 1));

      var stats = cut.FindAll("div.status-bar-horizontal div.stat-horizontal");
      Assert.Equal(4, stats.Count);

      Assert.Contains("VIE", stats[0].TextContent);
      Assert.Contains("MANA", stats[1].TextContent);
      Assert.Contains("SCORE", stats[2].TextContent);
      Assert.Contains("CLÉS", stats[3].TextContent);

      Assert.Contains("1,250", stats[2].TextContent);
      Assert.Contains("2", stats[3].TextContent);
    }
  }
}
