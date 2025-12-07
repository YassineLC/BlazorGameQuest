using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using ApiGateway.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Xunit;

namespace ApiGateway.Tests.Controllers
{
  public class AdminControllerTests
  {
    private static AppDbContext GetDbContext()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;

      return new AppDbContext(options);
    }

    [Fact]
    public async Task GetDashboardStats_ShouldReturnCorrectStatistics()
    {
      using var db = GetDbContext();
      
      // Setup test data
      var user = new User { Username = "TestPlayer", Email = "test@game.local", PasswordHash = "hash" };
      var dungeon = new Dungeon { Name = "TestDungeon", MaxDepth = 5 };
      db.Users.Add(user);
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var session = new GameSession 
      { 
        PlayerId = user.Id, 
        DungeonId = dungeon.Id, 
        CurrentScore = 100,
        IsFinished = true
      };
      db.GameSessions.Add(session);
      await db.SaveChangesAsync();

      var score = new Score 
      { 
        PlayerId = user.Id, 
        SessionId = session.Id, 
        Value = 100,
        AchievedAt = DateTime.UtcNow
      };
      db.Scores.Add(score);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.GetDashboardStats();

      // Assert
      var okResult = result.Result as OkObjectResult;
      okResult.Should().NotBeNull();
      
      var stats = okResult!.Value as DashboardStats;
      stats!.TotalPlayers.Should().Be(1);
      stats.TotalSessions.Should().Be(1);
      stats.TotalScores.Should().Be(1);
      stats.TotalDungeons.Should().Be(1);
      stats.AverageScore.Should().Be(100);
    }

    [Fact]
    public async Task GetPlayers_ShouldReturnAllPlayersWithStatistics()
    {
      using var db = GetDbContext();
      
      var player1 = new User { Username = "Player1", Email = "player1@game.local", PasswordHash = "hash" };
      var player2 = new User { Username = "Player2", Email = "player2@game.local", PasswordHash = "hash", Role = UserRole.Admin };
      
      db.Users.AddRange(player1, player2);
      await db.SaveChangesAsync();

      var score = new Score { PlayerId = player1.Id, Value = 500, AchievedAt = DateTime.UtcNow };
      db.Scores.Add(score);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.GetPlayers();

      // Assert
      var okResult = result.Result as OkObjectResult;
      okResult.Should().NotBeNull();
      
      var players = okResult!.Value as IEnumerable<PlayerAdminDto>;
      players.Should().HaveCount(2);
      
      var player1Info = players.Should().ContainSingle(p => p.Username == "Player1").Subject;
      player1Info.AverageScore.Should().Be(500);
      player1Info.IsActive.Should().BeTrue();
      
      var player2Info = players.Should().ContainSingle(p => p.Username == "Player2").Subject;
      player2Info.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DisablePlayer_ShouldChangeRoleToAdmin()
    {
      using var db = GetDbContext();
      
      var player = new User { Username = "TestPlayer", Email = "test@game.local", PasswordHash = "hash" };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.DisablePlayer(player.Id);

      // Assert
      result.Should().BeOfType<NoContentResult>();
      
      var updated = await db.Users.FindAsync(player.Id);
      updated!.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task EnablePlayer_ShouldChangeRoleToPlayer()
    {
      using var db = GetDbContext();
      
      var player = new User 
      { 
        Username = "TestPlayer", 
        Email = "test@game.local", 
        PasswordHash = "hash",
        Role = UserRole.Admin
      };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.EnablePlayer(player.Id);

      // Assert
      result.Should().BeOfType<NoContentResult>();
      
      var updated = await db.Users.FindAsync(player.Id);
      updated!.Role.Should().Be(UserRole.Player);
    }

    [Fact]
    public async Task GetScores_ShouldReturnAllScoresWithDetails()
    {
      using var db = GetDbContext();
      
      var player = new User { Username = "Player", Email = "player@game.local", PasswordHash = "hash" };
      var dungeon = new Dungeon { Name = "DungeonTest", MaxDepth = 5 };
      db.Users.Add(player);
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var session = new GameSession 
      { 
        PlayerId = player.Id, 
        DungeonId = dungeon.Id, 
        CurrentScore = 200,
        IsFinished = true
      };
      db.GameSessions.Add(session);
      await db.SaveChangesAsync();

      var score = new Score 
      { 
        PlayerId = player.Id, 
        SessionId = session.Id, 
        Value = 200,
        AchievedAt = DateTime.UtcNow
      };
      db.Scores.Add(score);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.GetScores();

      // Assert
      var okResult = result.Result as OkObjectResult;
      okResult.Should().NotBeNull();
      
      var scores = okResult!.Value as IEnumerable<ScoreAdminDto>;
      scores.Should().HaveCount(1);
      
      var scoreDto = scores.Should().ContainSingle().Subject;
      scoreDto.PlayerName.Should().Be("Player");
      scoreDto.DungeonName.Should().Be("DungeonTest");
      scoreDto.FinalScore.Should().Be(200);
    }

    [Fact]
    public async Task GetScoresStats_ShouldReturnAggregatedStatistics()
    {
      using var db = GetDbContext();
      
      var player = new User { Username = "Player", Email = "player@game.local", PasswordHash = "hash" };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      db.Scores.AddRange(
          new Score { PlayerId = player.Id, Value = 100, AchievedAt = DateTime.UtcNow },
          new Score { PlayerId = player.Id, Value = 200, AchievedAt = DateTime.UtcNow },
          new Score { PlayerId = player.Id, Value = 300, AchievedAt = DateTime.UtcNow }
      );
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.GetScoresStats();

      // Assert
      var okResult = result.Result as OkObjectResult;
      var stats = okResult!.Value as ScoresStatsDto;
      
      stats!.AverageScore.Should().Be(200);
      stats.MaxScore.Should().Be(300);
      stats.TotalScores.Should().Be(600);
    }

    [Fact]
    public async Task GetSessions_ShouldReturnAllSessions()
    {
      using var db = GetDbContext();
      
      var player = new User { Username = "Player", Email = "player@game.local", PasswordHash = "hash" };
      var dungeon = new Dungeon { Name = "Dungeon", MaxDepth = 5 };
      db.Users.Add(player);
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var session = new GameSession 
      { 
        PlayerId = player.Id, 
        DungeonId = dungeon.Id, 
        CurrentScore = 150,
        IsFinished = true,
        StartedAt = DateTime.UtcNow,
        EndedAt = DateTime.UtcNow.AddHours(1)
      };
      db.GameSessions.Add(session);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.GetSessions();

      // Assert
      var okResult = result.Result as OkObjectResult;
      var sessions = okResult!.Value as IEnumerable<SessionAdminDto>;
      
      sessions.Should().HaveCount(1);
      var sessionDto = sessions.Should().ContainSingle().Subject;
      sessionDto.PlayerName.Should().Be("Player");
      sessionDto.DungeonName.Should().Be("Dungeon");
      sessionDto.FinalScore.Should().Be(150);
    }

    [Fact]
    public async Task GetGlobalLeaderboard_ShouldReturnTopPlayers()
    {
      using var db = GetDbContext();
      
      var player1 = new User { Username = "TopPlayer", Email = "top@game.local", PasswordHash = "hash" };
      var player2 = new User { Username = "SecondPlayer", Email = "second@game.local", PasswordHash = "hash" };
      db.Users.AddRange(player1, player2);
      await db.SaveChangesAsync();

      db.Scores.AddRange(
          new Score { PlayerId = player1.Id, Value = 500, AchievedAt = DateTime.UtcNow },
          new Score { PlayerId = player2.Id, Value = 300, AchievedAt = DateTime.UtcNow }
      );
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.GetGlobalLeaderboard(10);

      // Assert
      var okResult = result.Result as OkObjectResult;
      var leaderboard = okResult!.Value as IEnumerable<LeaderboardEntryDto>;
      
      leaderboard.Should().HaveCount(2);
      var topPlayer = leaderboard!.First();
      topPlayer.PlayerName.Should().Be("TopPlayer");
      topPlayer.TotalScore.Should().Be(500);
    }

    [Fact]
    public async Task GetDungeons_ShouldReturnAllDungeonsWithStats()
    {
      using var db = GetDbContext();
      
      var dungeon = new Dungeon { Name = "TestDungeon", MaxDepth = 7 };
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var player = new User { Username = "Player", Email = "player@game.local", PasswordHash = "hash" };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      var room = new Room { DungeonId = dungeon.Id, Order = 1, Description = "Test" };
      db.Rooms.Add(room);
      await db.SaveChangesAsync();

      var session = new GameSession 
      { 
        PlayerId = player.Id, 
        DungeonId = dungeon.Id, 
        CurrentScore = 100,
        IsFinished = false
      };
      db.GameSessions.Add(session);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.GetDungeons();

      // Assert
      var okResult = result.Result as OkObjectResult;
      var dungeons = okResult!.Value as IEnumerable<DungeonAdminDto>;
      
      dungeons.Should().HaveCount(1);
      var dungeonDto = dungeons.Should().ContainSingle().Subject;
      dungeonDto.Name.Should().Be("TestDungeon");
      dungeonDto.MaxDepth.Should().Be(7);
      dungeonDto.RoomCount.Should().Be(1);
      dungeonDto.SessionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExportPlayers_ShouldReturnAllPlayers()
    {
      using var db = GetDbContext();
      
      var player = new User { Username = "ExportPlayer", Email = "export@game.local", PasswordHash = "hash" };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.ExportPlayers();

      // Assert
      result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ExportScores_ShouldReturnAllScores()
    {
      using var db = GetDbContext();
      
      var player = new User { Username = "Player", Email = "player@game.local", PasswordHash = "hash" };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      var score = new Score { PlayerId = player.Id, Value = 100, AchievedAt = DateTime.UtcNow };
      db.Scores.Add(score);
      await db.SaveChangesAsync();

      var controller = new AdminController(db);

      // Act
      var result = await controller.ExportScores();

      // Assert
      result.Should().BeOfType<OkObjectResult>();
    }
  }
}
