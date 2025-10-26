using System;
using FluentAssertions;
using SharedModels.Models;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Tests.Models
{
  public class TrapModelTests
  {
    [Fact]
    public void Trap_ShouldInitialize_WithValidDefaults()
    {
      var trap = new Trap();
      trap.Id.Should().NotBe(Guid.Empty);
      trap.Type.Should().Be(TrapType.Spike);
      trap.ChanceToTriggerPercent.Should().Be(50);
      trap.Damage.Should().Be(10);
      trap.CanBeDisarmed.Should().BeTrue();
      trap.IsDisarmed.Should().BeFalse();
      trap.RoomId.Should().Be(Guid.Empty);
      trap.Room.Should().BeNull();
    }

    [Fact]
    public void Trap_ShouldAllow_SettingAllProperties()
    {
      // Arrange
      var roomId = Guid.NewGuid();
      var trap = new Trap
      {
        Id = Guid.NewGuid(),
        Type = TrapType.Fire,
        ChanceToTriggerPercent = 80,
        Damage = 25,
        CanBeDisarmed = false,
        IsDisarmed = true,
        RoomId = roomId
      };

      // Assert
      trap.Type.Should().Be(TrapType.Fire);
      trap.ChanceToTriggerPercent.Should().Be(80);
      trap.Damage.Should().Be(25);
      trap.CanBeDisarmed.Should().BeFalse();
      trap.IsDisarmed.Should().BeTrue();
      trap.RoomId.Should().Be(roomId);
    }

    [Fact]
    public void Trap_CanLinkToRoom()
    {
      var room = new Room
      {
        Id = Guid.NewGuid(),
        Description = "Salle de feu"
      };

      var trap = new Trap
      {
        Room = room,
        RoomId = room.Id,
        Type = TrapType.Fire
      };
      trap.Room.Should().NotBeNull();
      trap.Room.Description.Should().Be("Salle de feu");
      trap.RoomId.Should().Be(room.Id);
      trap.Type.Should().Be(TrapType.Fire);
    }

    [Theory] //éxécution multiple avec des données différentes
    [InlineData(-10)] //première donnée
    [InlineData(150)] //deuxième donnée
    public void Trap_ChanceToTriggerPercent_ShouldBeInvalid_WhenOutOfRange(int invalidValue)
    {
      var trap = new Trap { ChanceToTriggerPercent = invalidValue };
      // Simuler la validation [Range]/[Required] du modèle dans le test unitaire
      var context = new ValidationContext(trap);
      var results = new List<ValidationResult>();
      var isValid = Validator.TryValidateObject(trap, context, results, true);

      isValid.Should().BeFalse();
      results.Should().Contain(r => r.ErrorMessage!.Contains("probabilité"));
    }


    [Fact]
    public void Trap_CanBeMarked_AsDisarmed()
    {
      var trap = new Trap { CanBeDisarmed = true };
      trap.IsDisarmed = true;
      trap.IsDisarmed.Should().BeTrue();
      trap.CanBeDisarmed.Should().BeTrue();
    }
  }
}
