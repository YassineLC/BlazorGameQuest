using System;
using FluentAssertions;
using SharedModels.Models;
using Xunit;

namespace SharedModels.Tests.Models
{
  public class GameSessionModelTests
  {
    [Fact]
    public void GameSession_ShouldInitialize_WithValidDefaults()
    {
      var session = new GameSession();

      session.Id.Should().NotBe(Guid.Empty);
      session.PlayerId.Should().Be(Guid.Empty);
      session.DungeonId.Should().Be(Guid.Empty);
      session.StartedAt.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
      session.EndedAt.Should().BeNull();
      session.CurrentScore.Should().Be(0);
      session.IsFinished.Should().BeFalse();
      session.Player.Should().BeNull();
      session.Dungeon.Should().BeNull();
    }

    [Fact]
    public void GameSession_ShouldAllow_SettingAllProperties()
    {
      var playerId = Guid.NewGuid();
      var dungeonId = Guid.NewGuid();
      var now = DateTime.UtcNow;

      var session = new GameSession
      {
        PlayerId = playerId,
        DungeonId = dungeonId,
        
        EndedAt = now,
        CurrentScore = 1200,
        IsFinished = true
      };

      session.PlayerId.Should().Be(playerId);
      session.DungeonId.Should().Be(dungeonId);
      session.StartedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
      session.EndedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
      session.CurrentScore.Should().Be(1200);
      session.IsFinished.Should().BeTrue();
    }

    [Fact]
    public void GameSession_ShouldLinkTo_Player_And_Dungeon()
    {
      var player = new User
      {
        Username = "Yassine",
        Email = "yassine@gmail.com",
        PasswordHash = "hash"
      };

      var dungeon = new Dungeon
      {
        Name = "Crypte des Ombres"
      };

      var session = new GameSession
      {
        Player = player,
        Dungeon = dungeon,
        CurrentScore = 500
      };

      session.Player.Should().NotBeNull();
      session.Dungeon.Should().NotBeNull();
      session.Player.Username.Should().Be("Yassine");
      session.Dungeon.Name.Should().Be("Crypte des Ombres");
      session.CurrentScore.Should().Be(500);
    }

    [Fact]
    public void GameSession_Ending_ShouldSet_IsFinished_True()
    {
      var session = new GameSession { IsFinished = false };

      session.EndedAt = DateTime.UtcNow;
      session.IsFinished = true;

      session.IsFinished.Should().BeTrue();
      session.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public void GameSession_CanTrack_ScoreProgression()
    {
      var session = new GameSession();

      session.CurrentScore += 250;
      session.CurrentScore += 100;

      session.CurrentScore.Should().Be(350);
    }
  }
}
