using System;
using FluentAssertions;
using SharedModels.Models;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace SharedModels.Tests.Models
{
  public class ItemModelTests
  {
    [Fact]
    public void Item_ShouldInitialize_WithValidDefaults()
    {
      var item = new Item();

      item.Id.Should().NotBe(Guid.Empty);
      item.Name.Should().BeEmpty();
      item.Description.Should().BeEmpty();
      item.Value.Should().Be(0);
      item.RoomId.Should().Be(Guid.Empty);
      item.Room.Should().BeNull();
    }

    [Fact]
    public void Item_ShouldAllow_SettingAllProperties()
    {
      var roomId = Guid.NewGuid();
      var item = new Item
      {
        Id = Guid.NewGuid(),
        Name = "Épée en acier",
        Description = "Une épée forgée dans un métal solide et bien équilibrée",
        Value = 150,
        RoomId = roomId
      };

      item.Name.Should().Be("Épée en acier");
      item.Description.Should().Contain("métal solide");
      item.Value.Should().Be(150);
      item.RoomId.Should().Be(roomId);
    }

    [Fact]
    public void Item_CanLinkTo_Room()
    {
      var room = new Room
      {
        Id = Guid.NewGuid(),
        Description = "Salle du trésor"
      };

      var item = new Item
      {
        Name = "Amulette dorée",
        Description = "Objet ancien serti de pierres précieuses",
        Value = 300,
        RoomId = room.Id,
        Room = room
      };

      item.Room.Should().NotBeNull();
      item.Room.Description.Should().Be("Salle du trésor");
      item.RoomId.Should().Be(room.Id);
      item.Value.Should().Be(300);
    }

    [Fact]
    public void Item_Value_Should_Be_Positive()
    {
      // Arrange
      var item = new Item { Value = -10 };

      // Act
      var context = new ValidationContext(item);
      var results = new List<ValidationResult>();
      var isValid = Validator.TryValidateObject(item, context, results, true);

      // Assert
      isValid.Should().BeFalse();
      results.Should().ContainSingle()
        .Which.ErrorMessage.Should().Be("La valeur d’un objet doit être positive");
    }

    [Fact]
    public void Item_NameAndDescription_ShouldBeEditable()
    {
      var item = new Item { Name = "Potion", Description = "Restaure la santé" };

      item.Name = "Grande Potion";
      item.Description = "Restaure complètement la santé";

      item.Name.Should().Be("Grande Potion");
      item.Description.Should().Contain("complètement");
    }
  }
}
