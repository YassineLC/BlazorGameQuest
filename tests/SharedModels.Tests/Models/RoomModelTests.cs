using System;
using FluentAssertions;
using SharedModels.Models;
using Xunit;

namespace SharedModels.Tests.Models
{
  public class RoomModelTests
  {
    [Fact]
    public void Room_Defaults_AreInitialized()
    {
      var room = new Room();

      room.Id.Should().NotBe(Guid.Empty);
      room.Description.Should().BeEmpty();
      room.Traps.Should().NotBeNull().And.BeEmpty();
      room.Items.Should().NotBeNull().And.BeEmpty();
      room.Order.Should().Be(0);
      room.PointsReward.Should().Be(0);
      room.Difficulty.Should().Be(RoomDifficulty.Easy);
    }

    [Fact]
    public void Room_ShouldAccept_ValidValues_And_Relations()
    {
      var dungeon = new Dungeon { Name = "Crypte ancienne" };

      var trap = new Trap
      {
        Type = TrapType.Fire,
        Damage = 25,
        ChanceToTriggerPercent = 70
      };

      var item = new Item
      {
        Name = "Amulette magique",
        Description = "Accorde une protection temporaire",
        Value = 100
      };

      var room = new Room
      {
        Order = 2,
        Difficulty = RoomDifficulty.Hard,
        Description = "Salle de la flamme éternelle",
        PointsReward = 200,
        Dungeon = dungeon,
        DungeonId = dungeon.Id
      };

      room.Traps.Add(trap);
      room.Items.Add(item);

      room.Traps.Should().HaveCount(1);
      room.Traps.First().Type.Should().Be(TrapType.Fire);
      room.Traps.First().Damage.Should().Be(25);
      room.Traps.First().ChanceToTriggerPercent.Should().Be(70);

      room.Items.Should().HaveCount(1);
      room.Items.First().Name.Should().Be("Amulette magique");
      room.Items.First().Value.Should().Be(100);

      room.Difficulty.Should().Be(RoomDifficulty.Hard);
      room.Dungeon.Should().NotBeNull();
      room.Description.Should().Contain("flamme");
      room.PointsReward.Should().Be(200);
    }
  }
}
