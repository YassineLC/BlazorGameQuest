using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Xunit;

namespace ApiGateway.Tests.Controllers
{
  public class ScoresControllerTests
  {
    private static AppDbContext GetDb()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;
      return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAll_Should_Return_All_Scores()
    {
      using var db = GetDb();
      var player = new User { Username = "Louisa", Email = "louisa@efrei.net", PasswordHash = "pwd" };
      db.Users.Add(player);
      db.Scores.AddRange(
          new Score { PlayerId = player.Id, Value = 100 },
          new Score { PlayerId = player.Id, Value = 200 }
      );
      await db.SaveChangesAsync();

      var ctrl = new ScoresController(db);
      var result = await ctrl.GetAll();

      Assert.NotNull(result.Value);
      Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task GetById_Should_Return_Score_When_Exists()
    {
      using var db = GetDb();
      var player = new User { Username = "Yassine", Email = "Yassine@efrei.net", PasswordHash = "hash" };
      var score = new Score { PlayerId = player.Id, Value = 300 };
      db.Users.Add(player);
      db.Scores.Add(score);
      await db.SaveChangesAsync();

      var ctrl = new ScoresController(db);
      var result = await ctrl.GetById(score.Id);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      var returned = Assert.IsType<Score>(ok.Value);
      Assert.Equal(300, returned.Value);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Missing()
    {
      using var db = GetDb();
      var ctrl = new ScoresController(db);

      var result = await ctrl.GetById(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Player_NotFound()
    {
      using var db = GetDb();
      var ctrl = new ScoresController(db);

      var score = new Score { PlayerId = Guid.NewGuid(), Value = 500 };

      var result = await ctrl.Create(score);

      Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Add_Score_When_Player_Exists()
    {
      using var db = GetDb();
      var player = new User { Username = "Yassine", Email = "yassine@efrei.net", PasswordHash = "pwd" };
      db.Users.Add(player);
      await db.SaveChangesAsync();

      var ctrl = new ScoresController(db);
      var score = new Score
      {
        PlayerId = player.Id,
        Value = 1500
      };

      var result = await ctrl.Create(score);

      var created = Assert.IsType<CreatedAtActionResult>(result.Result);
      var addedScore = Assert.IsType<Score>(created.Value);

      Assert.Equal(1500, addedScore.Value);
      Assert.Single(db.Scores);
    }

    [Fact]
    public async Task Update_Should_Modify_Existing_Score()
    {
      using var db = GetDb();
      var player = new User { Username = "Louisa", Email = "louisa@efrei.net", PasswordHash = "hash" };
      db.Users.Add(player);

      var score = new Score { PlayerId = player.Id, Value = 200 };
      db.Scores.Add(score);
      await db.SaveChangesAsync();

      var ctrl = new ScoresController(db);

      var updated = new Score
      {
        Id = score.Id,
        PlayerId = player.Id,
        Value = 999,
        AchievedAt = DateTime.UtcNow.AddDays(-1)
      };

      var result = await ctrl.Update(score.Id, updated);

      Assert.IsType<NoContentResult>(result);
      var reloaded = await db.Scores.FindAsync(score.Id);
      Assert.Equal(999, reloaded!.Value);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Score_Missing()
    {
      using var db = GetDb();
      var ctrl = new ScoresController(db);
      var fake = new Score { Id = Guid.NewGuid(), Value = 1000 };

      var result = await ctrl.Update(fake.Id, fake);

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Id_Mismatch()
    {
      using var db = GetDb();
      var ctrl = new ScoresController(db);

      var score = new Score { Id = Guid.NewGuid(), Value = 250 };

      var result = await ctrl.Update(Guid.NewGuid(), score);

      Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Delete_Should_Remove_Score_When_Exists()
    {
      using var db = GetDb();
      var score = new Score { Value = 1234 };
      db.Scores.Add(score);
      await db.SaveChangesAsync();

      var ctrl = new ScoresController(db);

      var result = await ctrl.Delete(score.Id);

      Assert.IsType<NoContentResult>(result);
      Assert.Empty(db.Scores);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Missing()
    {
      using var db = GetDb();
      var ctrl = new ScoresController(db);

      var result = await ctrl.Delete(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result);
    }
  }
}
