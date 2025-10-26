using System;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using SharedModels.Models;
using Xunit;
using FluentAssertions;

namespace ApiGateway.Tests.Controllers
{
  public class DungeonsControllerTests
  {
    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
      using var db = TestDb.New();
      var ctrl = new DungeonsController(db);

      var dungeon = new Dungeon { Name = "Donjon Mystique" };

      var result = await ctrl.Create(dungeon);

      var created = result.Result as CreatedAtActionResult;
      created.Should().NotBeNull();
      (created!.Value as Dungeon)!.Name.Should().Be("Donjon Mystique");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenNameMissing()
    {
      using var db = TestDb.New();
      var ctrl = new DungeonsController(db);

      var result = await ctrl.Create(new Dungeon { Name = "" });

      result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnDungeon_WhenExists()
    {
      using var db = TestDb.New();
      var dungeon = new Dungeon { Name = "Crypte Souterraine" };
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var ctrl = new DungeonsController(db);

      var result = await ctrl.GetById(dungeon.Id);

      var ok = result.Result as OkObjectResult;
      ok.Should().NotBeNull();
      (ok!.Value as Dungeon)!.Name.Should().Be("Crypte Souterraine");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenNotExists()
    {
      using var db = TestDb.New();
      var ctrl = new DungeonsController(db);

      var result = await ctrl.GetById(Guid.NewGuid());

      result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ShouldModifyDungeon_WhenValid()
    {
      using var db = TestDb.New();
      var dungeon = new Dungeon { Name = "Ancien Donjon" };
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var ctrl = new DungeonsController(db);
      var updated = new Dungeon { Id = dungeon.Id, Name = "Donjon Révisé", CreatedAt = DateTime.UtcNow };

      var result = await ctrl.Update(dungeon.Id, updated);

      result.Should().BeOfType<NoContentResult>();
      db.Dungeons.Find(dungeon.Id)!.Name.Should().Be("Donjon Révisé");
    }

    [Fact]
    public async Task Delete_ShouldRemoveDungeon_WhenExists()
    {
      using var db = TestDb.New();
      var dungeon = new Dungeon { Name = "Donjon à Supprimer" };
      db.Dungeons.Add(dungeon);
      await db.SaveChangesAsync();

      var ctrl = new DungeonsController(db);

      var result = await ctrl.Delete(dungeon.Id);

      result.Should().BeOfType<NoContentResult>();
      db.Dungeons.Find(dungeon.Id).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenNotExists()
    {
      using var db = TestDb.New();
      var ctrl = new DungeonsController(db);

      var result = await ctrl.Delete(Guid.NewGuid());

      result.Should().BeOfType<NotFoundResult>();
    }
  }
}
