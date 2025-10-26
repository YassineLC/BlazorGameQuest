using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Models;
using Xunit;
using ApiGateway.Tests; 

namespace ApiGateway.Tests.Controllers
{
  public class GameSessionsControllerTests
  {
    [Fact]
    public async Task GetAll_Should_Return_All_GameSessions()
    {
      using var db = TestDb.New();
      var player = new User { Username = "Yassine", Email = "Yassine@efrei.net", PasswordHash = "123" };
      var dungeon = new Dungeon { Name = "Caverne Maudite" };
      db.Users.Add(player);
      db.Dungeons.Add(dungeon);
      db.GameSessions.AddRange(
          new GameSession { PlayerId = player.Id, DungeonId = dungeon.Id, CurrentScore = 120 },
          new GameSession { PlayerId = player.Id, DungeonId = dungeon.Id, CurrentScore = 300 }
      );
      await db.SaveChangesAsync();

      var ctrl = new GameSessionsController(db);
      var result = await ctrl.GetAll();

      Assert.NotNull(result.Value);
      Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task GetById_Should_Return_Session_When_Exists()
    {
      using var db = TestDb.New();
      var player = new User { Username = "Louisa", Email = "louisa@efrei.net", PasswordHash = "pwd" };
      var dungeon = new Dungeon { Name = "Donjon des Ombres" };
      var session = new GameSession { PlayerId = player.Id, DungeonId = dungeon.Id, CurrentScore = 200 };

      db.Users.Add(player);
      db.Dungeons.Add(dungeon);
      db.GameSessions.Add(session);
      await db.SaveChangesAsync();

      var ctrl = new GameSessionsController(db);
      var result = await ctrl.GetById(session.Id);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      var returned = Assert.IsType<GameSession>(ok.Value);
      Assert.Equal(200, returned.CurrentScore);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new GameSessionsController(db);

      var result = await ctrl.GetById(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Player_NotFound()
    {
      using var db = TestDb.New();
      var dungeon = new Dungeon { Name = "Temple Perdu" };
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var ctrl = new GameSessionsController(db);
      var session = new GameSession { PlayerId = Guid.NewGuid(), DungeonId = dungeon.Id };

      var result = await ctrl.Create(session);

      var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
      Assert.Contains("joueur associé n'existe pas", bad.Value!.ToString()!);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Dungeon_NotFound()
    {
      using var db = TestDb.New();
      var player = new User { Username = "RandomPlayer", Email = "randomplayer@mail.com", PasswordHash = "pwd" };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      var ctrl = new GameSessionsController(db);
      var session = new GameSession { PlayerId = player.Id, DungeonId = Guid.NewGuid() };

      var result = await ctrl.Create(session);

      var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
      Assert.Contains("donjon associé n'existe pas", bad.Value!.ToString()!);
    }

    [Fact]
    public async Task Create_Should_Add_Session_When_Valid()
    {
      using var db = TestDb.New();
      var player = new User { Username = "Neo", Email = "neo@mail.com", PasswordHash = "pwd" };
      var dungeon = new Dungeon { Name = "Forteresse du Temps" };
      db.Users.Add(player);
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var ctrl = new GameSessionsController(db);
      var session = new GameSession
      {
        PlayerId = player.Id,
        DungeonId = dungeon.Id,
        CurrentScore = 500
      };

      var result = await ctrl.Create(session);

      var created = Assert.IsType<CreatedAtActionResult>(result.Result);
      var added = Assert.IsType<GameSession>(created.Value);
      Assert.Equal(500, added.CurrentScore);
      Assert.Single(db.GameSessions);
    }

    [Fact]
    public async Task Update_Should_Modify_Existing_Session()
    {
      using var db = TestDb.New();
      var player = new User { Username = "Louisa", Email = "louisa@mail.com", PasswordHash = "pwd" };
      var dungeon = new Dungeon { Name = "Caverne Mystique" };
      db.Users.Add(player);
      db.Dungeons.Add(dungeon);

      var session = new GameSession
      {
        PlayerId = player.Id,
        DungeonId = dungeon.Id,
        CurrentScore = 200,
        IsFinished = false
      };
      db.GameSessions.Add(session);
      await db.SaveChangesAsync();

      var ctrl = new GameSessionsController(db);
      var input = new GameSession
      {
        Id = session.Id,
        PlayerId = player.Id,
        DungeonId = dungeon.Id,
        CurrentScore = 400,
        IsFinished = true
      };

      var result = await ctrl.Update(session.Id, input);

      Assert.IsType<NoContentResult>(result);
      var updated = await db.GameSessions.FindAsync(session.Id);
      Assert.Equal(400, updated!.CurrentScore);
      Assert.True(updated.IsFinished);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new GameSessionsController(db);

      var fake = new GameSession { Id = Guid.NewGuid(), CurrentScore = 100 };
      var result = await ctrl.Update(fake.Id, fake);

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Id_Mismatch()
    {
      using var db = TestDb.New();
      var ctrl = new GameSessionsController(db);

      var session = new GameSession { Id = Guid.NewGuid(), CurrentScore = 200 };
      var result = await ctrl.Update(Guid.NewGuid(), session);

      Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Delete_Should_Remove_Session_When_Exists()
    {
      using var db = TestDb.New();
      var player = new User { Username = "Yassine", Email = "yassine@mail.com", PasswordHash = "pwd" };
      var dungeon = new Dungeon { Name = "Ruines Antiques" };
      db.Users.Add(player);
      db.Dungeons.Add(dungeon);
      var session = new GameSession { PlayerId = player.Id, DungeonId = dungeon.Id };
      db.GameSessions.Add(session);
      await db.SaveChangesAsync();

      var ctrl = new GameSessionsController(db);
      var result = await ctrl.Delete(session.Id);

      Assert.IsType<NoContentResult>(result);
      Assert.Empty(db.GameSessions);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new GameSessionsController(db);

      var result = await ctrl.Delete(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result);
    }
  }
}
