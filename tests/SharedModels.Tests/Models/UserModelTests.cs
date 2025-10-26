using System;
using System.Linq;
using FluentAssertions;
using SharedModels.Models;
using Xunit;

namespace SharedModels.Tests.Models
{
  public class UserModelTests
  {
    [Fact]
    public void User_ShouldInitialize_WithValidDefaults()
    {
      // Act
      var user = new User();

      // Assert
      user.Id.Should().NotBe(Guid.Empty);
      user.Username.Should().BeEmpty();
      user.Email.Should().BeEmpty();
      user.PasswordHash.Should().BeEmpty();
      user.Role.Should().Be(UserRole.Player);
      user.GameSessions.Should().NotBeNull();
      user.Scores.Should().NotBeNull();
    }

    [Fact]
    public void User_ShouldAllow_SettingAllProperties()
    {
      // Arrange
      var id = Guid.NewGuid();
      var user = new User
      {
        Id = id,
        Username = "Yassine",
        Email = "yassine@gmail.com",
        PasswordHash = "hashedpass123",
        Role = UserRole.Admin
      };

      // Assert
      user.Id.Should().Be(id);
      user.Username.Should().Be("Yassine");
      user.Email.Should().Be("yassine@gmail.com");
      user.PasswordHash.Should().Be("hashedpass123");
      user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    // le User doit contenir ses GameSessions et Scores
    public void User_ShouldLink_GameSessions_And_Scores()
    {
      var session = new GameSession { CurrentScore = 500 };
      var score = new Score { Value = 250 };

      var user = new User
      {
        Username = "PlayerOne",
        Email = "player@efrei.net",
        PasswordHash = "hashed123",
        GameSessions = { session },
        Scores = { score }
      };
      user.GameSessions.Should().HaveCount(1);
      user.GameSessions.First().CurrentScore.Should().Be(500);

      user.Scores.Should().HaveCount(1);
      user.Scores.First().Value.Should().Be(250);
    }

    [Fact]
    public void UserRole_ShouldDefaultTo_Player()
    {
      var user = new User();
      user.Role.Should().Be(UserRole.Player);
    }

    [Fact]
    public void UserRole_ShouldSwitchTo_Admin()
    {
      var user = new User { Role = UserRole.Admin };
      user.Role.Should().Be(UserRole.Admin);
    }
  }
}
