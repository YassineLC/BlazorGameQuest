using System;
using FluentAssertions;
using SharedModels.Models;
using Xunit;

namespace SharedModels.Tests.Models
{
  public class ScoreModelTests
  {
    [Fact]
    public void Score_ShouldInitialize_WithValidDefaults()
    {
      // Act
      var score = new Score();

      // Assert
      score.Id.Should().NotBe(Guid.Empty);
      score.PlayerId.Should().Be(Guid.Empty);
      score.Player.Should().BeNull();
      score.Value.Should().Be(0);
      score.SessionId.Should().BeNull();
      score.AchievedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1)); // date valide
    }

    [Fact]
    public void Score_ShouldAllow_SettingAllProperties()
    {
      // Arrange
      var playerId = Guid.NewGuid();
      var sessionId = Guid.NewGuid();
      var now = DateTime.UtcNow;

      var score = new Score
      {
        PlayerId = playerId,
        Value = 500,
        AchievedAt = now,
        SessionId = sessionId
      };

      // Assert
      score.PlayerId.Should().Be(playerId);
      score.Value.Should().Be(500);
      score.AchievedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
      score.SessionId.Should().Be(sessionId);
    }

    [Fact]
    public void Score_ShouldLinkTo_Player()
    {
      var player = new User
      {
        Username = "Louisa",
        Email = "louisa@bytel.fr",
        PasswordHash = "hashedpass"
      };

      var score = new Score
      {
        Player = player,
        PlayerId = player.Id,
        Value = 1200
      };
      score.Player.Should().NotBeNull();
      score.Player.Username.Should().Be("Louisa");
      score.Player.Email.Should().Be("louisa@bytel.fr");
      score.Value.Should().Be(1200);
    }

    [Fact]
    public void Score_AchievedAt_ShouldBeRecent()
    {
      var score = new Score();
      var timeSinceCreation = DateTime.UtcNow - score.AchievedAt;
      timeSinceCreation.Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Score_CanBeAssociatedWith_Session()
    {
      var sessionId = Guid.NewGuid();

      var score = new Score
      {
        Value = 300,
        SessionId = sessionId
      };
      score.SessionId.Should().Be(sessionId);
      score.Value.Should().Be(300);
    }
  }
}
