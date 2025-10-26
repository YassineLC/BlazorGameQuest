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
  public class TrapsControllerTests
  {
    [Fact]
    public async Task GetAll_Should_Return_All_Traps()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle du feu" };
      db.Rooms.Add(room);
      db.Traps.AddRange(
          new Trap { RoomId = room.Id, Type = TrapType.Fire },
          new Trap { RoomId = room.Id, Type = TrapType.Spike }
      );
      await db.SaveChangesAsync();

      var ctrl = new TrapsController(db);
      var result = await ctrl.GetAll();

      Assert.NotNull(result.Value);
      Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task GetById_Should_Return_Trap_When_Found()
    {
      using var db = TestDb.New();
      var trap = new Trap { Type = TrapType.Poison, RoomId = Guid.NewGuid() };
      db.Traps.Add(trap);
      await db.SaveChangesAsync();

      var ctrl = new TrapsController(db);
      var result = await ctrl.GetById(trap.Id);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      var returnedTrap = Assert.IsType<Trap>(ok.Value);
      Assert.Equal(TrapType.Poison, returnedTrap.Type);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new TrapsController(db);

      var result = await ctrl.GetById(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Room_NotFound()
    {
      using var db = TestDb.New();
      var ctrl = new TrapsController(db);

      var trap = new Trap { RoomId = Guid.NewGuid(), Type = TrapType.Spike };
      var result = await ctrl.Create(trap);

      Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Add_Trap_When_Room_Exists()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle empoisonnée" };
      db.Rooms.Add(room);
      await db.SaveChangesAsync();

      var ctrl = new TrapsController(db);
      var trap = new Trap
      {
        RoomId = room.Id,
        Type = TrapType.Poison,
        Damage = 25,
        ChanceToTriggerPercent = 60
      };

      var result = await ctrl.Create(trap);

      var created = Assert.IsType<CreatedAtActionResult>(result.Result);
      var addedTrap = Assert.IsType<Trap>(created.Value);

      Assert.Equal(TrapType.Poison, addedTrap.Type);
      Assert.Single(db.Traps);
    }

    [Fact]
    public async Task Update_Should_Modify_Existing_Trap()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle de test" };
      db.Rooms.Add(room);

      var trap = new Trap { RoomId = room.Id, Type = TrapType.Spike, Damage = 10 };
      db.Traps.Add(trap);
      await db.SaveChangesAsync();

      var ctrl = new TrapsController(db);

      var update = new Trap
      {
        Id = trap.Id,
        RoomId = room.Id,
        Type = TrapType.Fire,
        Damage = 50,
        CanBeDisarmed = false,
        IsDisarmed = true
      };

      var result = await ctrl.Update(trap.Id, update);

      Assert.IsType<NoContentResult>(result);
      var updated = await db.Traps.FindAsync(trap.Id);
      Assert.Equal(TrapType.Fire, updated!.Type);
      Assert.True(updated.IsDisarmed);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Trap_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new TrapsController(db);

      var trap = new Trap { Id = Guid.NewGuid(), Type = TrapType.Snare };

      var result = await ctrl.Update(trap.Id, trap);

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Id_Mismatch()
    {
      using var db = TestDb.New();
      var ctrl = new TrapsController(db);

      var trap = new Trap { Id = Guid.NewGuid(), Type = TrapType.Spike };

      var result = await ctrl.Update(Guid.NewGuid(), trap);

      Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Delete_Should_Remove_Trap_When_Found()
    {
      using var db = TestDb.New();
      var trap = new Trap { Type = TrapType.Explosion, RoomId = Guid.NewGuid() };
      db.Traps.Add(trap);
      await db.SaveChangesAsync();

      var ctrl = new TrapsController(db);

      var result = await ctrl.Delete(trap.Id);

      Assert.IsType<NoContentResult>(result);
      Assert.Empty(db.Traps);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new TrapsController(db);

      var result = await ctrl.Delete(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result);
    }
  }
}