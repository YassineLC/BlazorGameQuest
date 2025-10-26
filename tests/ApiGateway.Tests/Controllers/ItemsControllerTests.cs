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
  public class ItemsControllerTests
  {
    [Fact]
    public async Task GetAll_Should_Return_All_Items()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle du trésor" };
      db.Rooms.Add(room);
      db.Items.AddRange(
          new Item { Name = "Épée", Value = 50, RoomId = room.Id },
          new Item { Name = "Potion", Value = 20, RoomId = room.Id }
      );
      await db.SaveChangesAsync();

      var ctrl = new ItemsController(db);
      var result = await ctrl.GetAll();

      Assert.NotNull(result.Value);
      Assert.Equal(2, result.Value.Count());
    }

    [Fact]
    public async Task GetById_Should_Return_Item_When_Exists()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle de test" };
      db.Rooms.Add(room);
      var item = new Item { Name = "Clé dorée", Value = 100, RoomId = room.Id };
      db.Items.Add(item);
      await db.SaveChangesAsync();

      var ctrl = new ItemsController(db);
      var result = await ctrl.GetById(item.Id);

      var ok = Assert.IsType<OkObjectResult>(result.Result);
      var returned = Assert.IsType<Item>(ok.Value);
      Assert.Equal("Clé dorée", returned.Name);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new ItemsController(db);

      var result = await ctrl.GetById(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Room_NotFound()
    {
      using var db = TestDb.New();
      var ctrl = new ItemsController(db);

      var item = new Item { Name = "Épée", RoomId = Guid.NewGuid() };
      var result = await ctrl.Create(item);

      Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_Should_Add_Item_When_Room_Exists()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle A" };
      db.Rooms.Add(room);
      await db.SaveChangesAsync();

      var ctrl = new ItemsController(db);
      var item = new Item
      {
        RoomId = room.Id,
        Name = "Bouclier",
        Description = "Bouclier solide",
        Value = 80
      };

      var result = await ctrl.Create(item);

      var created = Assert.IsType<CreatedAtActionResult>(result.Result);
      var addedItem = Assert.IsType<Item>(created.Value);

      Assert.Equal("Bouclier", addedItem.Name);
      Assert.Single(db.Items);
    }

    [Fact]
    public async Task Update_Should_Modify_Existing_Item()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle 1" };
      db.Rooms.Add(room);

      var item = new Item { Name = "Arc", Description = "Ancien arc", Value = 60, RoomId = room.Id };
      db.Items.Add(item);
      await db.SaveChangesAsync();

      var ctrl = new ItemsController(db);
      var input = new Item
      {
        Id = item.Id,
        Name = "Arc long",
        Description = "Arc amélioré",
        Value = 120,
        RoomId = room.Id
      };

      var result = await ctrl.Update(item.Id, input);

      Assert.IsType<NoContentResult>(result);
      var updated = await db.Items.FindAsync(item.Id);
      Assert.Equal("Arc long", updated!.Name);
      Assert.Equal(120, updated.Value);
    }

    [Fact]
    public async Task Update_Should_Return_NotFound_When_Item_Not_Exists()
    {
      using var db = TestDb.New();
      var ctrl = new ItemsController(db);

      var fake = new Item { Id = Guid.NewGuid(), Name = "Faux objet" };
      var result = await ctrl.Update(fake.Id, fake);

      Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Id_Mismatch()
    {
      using var db = TestDb.New();
      var ctrl = new ItemsController(db);

      var item = new Item { Id = Guid.NewGuid(), Name = "Objet test" };
      var result = await ctrl.Update(Guid.NewGuid(), item);

      Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Delete_Should_Remove_Item_When_Exists()
    {
      using var db = TestDb.New();
      var room = new Room { Description = "Salle 2" };
      db.Rooms.Add(room);
      var item = new Item { Name = "Potion de soin", Value = 25, RoomId = room.Id };
      db.Items.Add(item);
      await db.SaveChangesAsync();

      var ctrl = new ItemsController(db);
      var result = await ctrl.Delete(item.Id);

      Assert.IsType<NoContentResult>(result);
      Assert.Empty(db.Items);
    }

    [Fact]
    public async Task Delete_Should_Return_NotFound_When_Missing()
    {
      using var db = TestDb.New();
      var ctrl = new ItemsController(db);

      var result = await ctrl.Delete(Guid.NewGuid());

      Assert.IsType<NotFoundResult>(result);
    }
  }
}
