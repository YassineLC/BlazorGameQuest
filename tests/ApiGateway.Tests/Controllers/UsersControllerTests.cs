using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using ApiGateway.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedModels.Models;
using Xunit;

namespace ApiGateway.Tests.Controllers
{
  public class UsersControllerTests
  {
    private static AppDbContext GetDbContext()
    {
      var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString()) 
          .Options;

      return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllUsers()
    {
      using var db = GetDbContext();
      db.Users.AddRange(
          new User { Username = "Louisa", Email = "Louisa@efrei.net", PasswordHash = "pass" },
          new User { Username = "Yassine", Email = "Yassine@efrei.net", PasswordHash = "pass" }
      );
      await db.SaveChangesAsync();

      var controller = new UsersController(db);

      var result = await controller.GetAll();

      result.Value.Should().HaveCount(2);
      result.Value.Should().Contain(u => u.Username == "Louisa");
      result.Value.Should().Contain(u => u.Username == "Yassine");
    }

    [Fact]
    public async Task GetById_ShouldReturnUser_WhenExists()
    {
      using var db = GetDbContext();
      var user = new User { Username = "Riles", Email = "Riles@jam.fr", PasswordHash = "pwd" };
      db.Users.Add(user);
      await db.SaveChangesAsync();

      var controller = new UsersController(db);

      var result = await controller.GetById(user.Id);

      var ok = result.Result as OkObjectResult; //récupérer la réponse HTTP 200 OK 
      ok.Should().NotBeNull();
      (ok!.Value as User)!.Username.Should().Be("Riles");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
      using var db = GetDbContext();
      var controller = new UsersController(db);

      var result = await controller.GetById(Guid.NewGuid());

      result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_ShouldAddUser_WhenValid()
    {
      using var db = GetDbContext();
      var controller = new UsersController(db);
      var newUser = new User
      {
        Username = "taylor",
        Email = "taylor@swift.com",
        PasswordHash = "securepwd"
      };

      var result = await controller.Create(newUser);

      var created = result.Result as CreatedAtActionResult;
      created.Should().NotBeNull();
      created!.Value.Should().BeOfType<User>();

      db.Users.Should().ContainSingle(u => u.Username == "taylor");
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenMissingUsernameOrEmail()
    {
      using var db = GetDbContext();
      var controller = new UsersController(db);

      var invalidUser = new User { Username = "", Email = "", PasswordHash = "pwd" };

      var result = await controller.Create(invalidUser);

      result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_ShouldModifyExistingUser()
    {
      using var db = GetDbContext();
      var user = new User { Username = "OldName", Email = "old@test.com", PasswordHash = "pwd" };
      db.Users.Add(user);
      await db.SaveChangesAsync();

      var controller = new UsersController(db);

      var updated = new User
      {
        Id = user.Id,
        Username = "NewName",
        Email = "new@test.com",
        PasswordHash = "newpwd",
        Role = UserRole.Admin
      };

      var result = await controller.Update(user.Id, updated);

      result.Should().BeOfType<NoContentResult>();
      var reloaded = await db.Users.FindAsync(user.Id);
      reloaded!.Username.Should().Be("NewName");
      reloaded.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
      using var db = GetDbContext();
      var controller = new UsersController(db);

      var updated = new User
      {
        Id = Guid.NewGuid(),
        Username = "Test",
        Email = "test@test.com",
        PasswordHash = "hash"
      };

      var result = await controller.Update(updated.Id, updated);

      result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenRouteIdDiffers()
    {
      using var db = GetDbContext();
      var controller = new UsersController(db);
      var user = new User { Id = Guid.NewGuid(), Username = "User", Email = "user@test.com" };

      var result = await controller.Update(Guid.NewGuid(), user);

      result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ShouldRemoveUser_WhenExists()
    {
      using var db = GetDbContext();
      var user = new User { Username = "DeleteMe", Email = "del@test.com", PasswordHash = "hash" };
      db.Users.Add(user);
      await db.SaveChangesAsync();

      var controller = new UsersController(db);

      var result = await controller.Delete(user.Id);

      result.Should().BeOfType<NoContentResult>();
      (await db.Users.FindAsync(user.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
      using var db = GetDbContext();
      var controller = new UsersController(db);

      var result = await controller.Delete(Guid.NewGuid());

      result.Should().BeOfType<NotFoundResult>();
    }
  }
}
