using System;
using FluentAssertions;
using SharedModels.Models;
using Xunit;

namespace SharedModels.Tests.Models
{
  public class DungeonModelTests
  {
    [Fact]
    public void Dungeon_Defaults_AreInitialized()
    {
      var d = new Dungeon();

      d.Id.Should().NotBe(Guid.Empty);
      d.Name.Should().BeEmpty();
      d.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
      d.Rooms.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Dungeon_ShouldAccept_Values_And_Rooms()
    {
      var dungeon = new Dungeon { Name = "Crypte des Échos" };

      var room = new Room
      {
        Order = 1,
        Description = "Entrée froide",
        DungeonId = dungeon.Id,
        Dungeon = dungeon
      };

      dungeon.Rooms.Add(room);

      dungeon.Name.Should().Be("Crypte des Échos");
      dungeon.Rooms.Should().HaveCount(1);

      var addedRoom = dungeon.Rooms.First();
      addedRoom.Dungeon.Should().BeSameAs(dungeon);
      addedRoom.DungeonId.Should().Be(dungeon.Id);
    }
  }
}
