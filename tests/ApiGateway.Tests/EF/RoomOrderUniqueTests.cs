using System;
using System.Threading.Tasks;
using ApiGateway.Tests;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Xunit;

namespace ApiGateway.Tests.EF;

public class RoomOrderUniqueTests
{
  [Fact]
  public async Task Rooms_WithSameOrder_InSameDungeon_ShouldFail()
  {
    var (db, conn) = TestDb.NewRelational();
    try
    {
      var d = new Dungeon { Name = "D1" };
      db.Dungeons.Add(d);

      db.Rooms.Add(new Room { DungeonId = d.Id, Order = 1, Description = "A" });
      db.Rooms.Add(new Room { DungeonId = d.Id, Order = 1, Description = "B" });

      await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
    finally
    {
      await db.DisposeAsync();
      await conn.DisposeAsync();
    }
  }

  [Fact]
  public async Task Rooms_SameOrder_InDifferentDungeons_ShouldSucceed()
  {
    var (db, conn) = TestDb.NewRelational();
    try
    {
      var d1 = new Dungeon { Name = "D1" };
      var d2 = new Dungeon { Name = "D2" };
      db.Dungeons.AddRange(d1, d2);

      db.Rooms.Add(new Room { DungeonId = d1.Id, Order = 1, Description = "A" });
      db.Rooms.Add(new Room { DungeonId = d2.Id, Order = 1, Description = "B" });

      await db.SaveChangesAsync(); 
    }
    finally
    {
      await db.DisposeAsync();
      await conn.DisposeAsync();
    }
  }
}
