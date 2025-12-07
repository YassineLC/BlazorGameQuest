using System;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Xunit;

namespace ApiGateway.Tests.Controllers
{
  public class PlayControllerTests
  {
    private static AppDbContext GetDb()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;
      return new AppDbContext(options);
    }

    [Fact]
    public async Task StartSession_Creates_Session_For_Guest_When_PlayerId_Null()
    {
      using var db = GetDb();

      var dungeon = new Dungeon { Name = "D1" };
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var ctrl = new PlayController(db);
      var req = new StartSessionRequest(null, dungeon.Id, null, null);

      var result = await ctrl.StartSession(req);

      var created = Assert.IsType<CreatedAtActionResult>(result.Result);
      var session = Assert.IsType<GameSession>(created.Value);
      Assert.Equal(dungeon.Id, session.DungeonId);
      Assert.False(session.IsFinished);
    }

    [Fact]
    public async Task EndSession_Persists_Score_And_Marks_Session_Finished()
    {
      using var db = GetDb();

      var player = new User { Username = "P", Email = "p@local", PasswordHash = "pwd" };
      db.Users.Add(player);

      var dungeon = new Dungeon { Name = "D2" };
      db.Dungeons.Add(dungeon);

      var session = new GameSession { PlayerId = player.Id, DungeonId = dungeon.Id, CurrentScore = 777, IsFinished = false };
      db.GameSessions.Add(session);

      await db.SaveChangesAsync();

      var ctrl = new PlayController(db);
      var result = await ctrl.EndSession(new EndSessionRequest(session.Id));

      Assert.IsType<NoContentResult>(result);

      var reloaded = await db.GameSessions.FindAsync(session.Id);
      Assert.True(reloaded!.IsFinished);

      var score = await db.Scores.FirstOrDefaultAsync(s => s.SessionId == session.Id);
      Assert.NotNull(score);
      Assert.Equal(777, score!.Value);
    }

    [Fact]
    public async Task VisitRoom_Returns_BadRequest_When_Session_NotFound()
    {
      using var db = GetDb();
      var ctrl = new PlayController(db);

      var req = new VisitRoomRequest(Guid.NewGuid(), Guid.NewGuid(), "explore");
      var result = await ctrl.VisitRoom(req);

      Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task VisitRoom_Returns_BadRequest_When_Room_NotFound()
    {
      using var db = GetDb();

      var player = new User { Username = "T", Email = "t@local", PasswordHash = "pwd" };
      db.Users.Add(player);

      var dungeon = new Dungeon { Name = "D3" };
      db.Dungeons.Add(dungeon);

      var session = new GameSession { PlayerId = player.Id, DungeonId = dungeon.Id };
      db.GameSessions.Add(session);

      await db.SaveChangesAsync();

      var ctrl = new PlayController(db);

      var req = new VisitRoomRequest(session.Id, Guid.NewGuid(), "explore");
      var result = await ctrl.VisitRoom(req);

      Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task VisitRoom_BossFailure_Persists_Score_When_Session_Finishes()
    {
      using var db = GetDb();

      var player = new User { Username = "BossTester", Email = "boss@local", PasswordHash = "pwd" };
      db.Users.Add(player);

      var dungeon = new Dungeon { Name = "BossDungeon" };
      db.Dungeons.Add(dungeon);

      var bossRoom = new Room
      {
        Name = "Boss Lair",
        Type = RoomType.Boss,
        Difficulty = RoomDifficulty.Hard,
        PointsReward = 500,
        NextRoomIds = new System.Collections.Generic.List<Guid>()
      };

      db.Rooms.Add(bossRoom);

      var session = new GameSession { PlayerId = player.Id, DungeonId = dungeon.Id, CurrentScore = 0, IsFinished = false };
      db.GameSessions.Add(session);

      await db.SaveChangesAsync();

      var ctrl = new PlayController(db);

      var req = new VisitRoomRequest(session.Id, bossRoom.Id, "flee");

      VisitResult? last = null;
      bool finished = false;

      for (int i = 0; i < 50; i++)
      {
        var res = await ctrl.VisitRoom(req);
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        var vr = Assert.IsType<VisitResult>(ok.Value);
        last = vr;
        if (vr.IsFinished)
        {
          finished = true;
          break;
        }
      }

      Assert.True(finished, "Expected the boss visit to eventually finish the session (randomized). Consider increasing retries if this fails intermittently.");

      var reloaded = await db.GameSessions.FindAsync(session.Id);
      Assert.True(reloaded!.IsFinished);

      var score = await db.Scores.FirstOrDefaultAsync(s => s.SessionId == session.Id);
      Assert.NotNull(score);
      Assert.True(score!.Value > 0);
    }
  }
}
