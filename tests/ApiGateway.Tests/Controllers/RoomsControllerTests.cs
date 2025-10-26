using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using ApiGateway.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Xunit;
using ApiGateway.Tests;

namespace ApiGateway.Tests.Controllers
{
  public class RoomsControllerTests
  {
    [Fact]
    public async Task GetAll_Should_Return_All_Rooms()
    {
      using var db = TestDb.New();
      var dungeon = new Dungeon { Name = "Forteresse du Nord" };
      db.Dungeons.Add(dungeon);
      db.Rooms.AddRange(
          new Room { DungeonId = dungeon.Id, Order = 1, Description = "Entrée" },
          new Room { DungeonId = dungeon.Id, Order = 2, Description = "Salle du Trésor" }
      );
      await db.SaveChangesAsync();

      var ctrl = new RoomsController(db);
      var result = await ctrl.GetAll();

      Assert.NotNull(result.Value);
      Assert.Equal(2, result.Value.Count());
      Assert.Contains(result.Value, r => r.Description == "Entrée");
    }

    [Fact]
    public async Task GetById_Should_Return_Room_When_Exists()
    {
      using var db = TestDb.New();
      var dungeon = new Dungeon { Name = "Caverne Mystique" };
      var room = new Room { DungeonId = dungeon.Id, Description = "Salle centrale", Order = 1 };
      db.Dungeons.Add(dungeon);
      db.Rooms.Add(room);
      await db.SaveChangesAsync();

      var ctrl = new RoomsController(db);
      var result = await ctrl.GetById(room.Id);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      var returnedRoom = Assert.IsType<Room>(ok.Value);
      Assert.Equal("Salle centrale", returnedRoom.Description);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Room_Not_Exists()
    {
      using var db = TestDb.New();
      var ctrl = new RoomsController(db);

      var result = await ctrl.GetById(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Fail_When_Dungeon_NotFound()
    {
      using var db = TestDb.New();
      var ctrl = new RoomsController(db);

      var room = new Room { DungeonId = Guid.NewGuid(), Order = 1, Description = "Inconnue" };
      var result = await ctrl.Create(room);

      Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Succeed_When_Dungeon_Exists()
    {
      using var db = TestDb.New();
      var d = new Dungeon { Name = "D1" };
      db.Dungeons.Add(d);
      await db.SaveChangesAsync();

      var ctrl = new RoomsController(db);
      var room = new Room { DungeonId = d.Id, Order = 1, Description = "Entrée" };
      var res = await ctrl.Create(room);

      var created = Assert.IsType<CreatedAtActionResult>(res.Result);
      var createdRoom = Assert.IsType<Room>(created.Value);
      Assert.Equal("Entrée", createdRoom.Description);
    }

    [Fact]
    public async Task Update_Should_Modify_Room_When_Exists()
    {
      using var db = TestDb.New();
      var d = new Dungeon { Name = "D1" };
      db.Dungeons.Add(d);
      var r = new Room { DungeonId = d.Id, Order = 1, Description = "Ancienne" };
      db.Rooms.Add(r);
      await db.SaveChangesAsync();

      var ctrl = new RoomsController(db);
      var input = new Room
      {
        Id = r.Id,
        DungeonId = d.Id,
        Order = 2,
        Description = "Nouvelle",
        PointsReward = 120
      };

      var res = await ctrl.Update(r.Id, input);

      Assert.IsType<NoContentResult>(res);
      var reload = await db.Rooms.FindAsync(r.Id);
      Assert.Equal("Nouvelle", reload!.Description);
      Assert.Equal(2, reload.Order);
      Assert.Equal(120, reload.PointsReward);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new RoomsController(db);

      var fake = new Room { Id = Guid.NewGuid(), Description = "X" };
      var res = await ctrl.Update(fake.Id, fake);

      Assert.IsType<NotFoundResult>(res);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Id_Mismatch()
    {
      using var db = TestDb.New();
      var ctrl = new RoomsController(db);

      var body = new Room { Id = Guid.NewGuid(), Description = "X" };
      var res = await ctrl.Update(Guid.NewGuid(), body);

      Assert.IsType<BadRequestResult>(res);
    }

    [Fact]
    public async Task Delete_Should_Remove_Room()
    {
      using var db = TestDb.New();
      var d = new Dungeon { Name = "D1" };
      db.Dungeons.Add(d);
      var r = new Room { DungeonId = d.Id, Description = "A supprimer" };
      db.Rooms.Add(r);
      await db.SaveChangesAsync();

      var ctrl = new RoomsController(db);
      var res = await ctrl.Delete(r.Id);

      Assert.IsType<NoContentResult>(res);
      Assert.Empty(db.Rooms);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new RoomsController(db);

      var res = await ctrl.Delete(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(res);
    }
  }
}
