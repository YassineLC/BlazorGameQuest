using System;
using ApiGateway.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ApiGateway.Tests;

internal static class TestDb
{
  public static AppDbContext New() =>
      new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .EnableSensitiveDataLogging()
          .Options);

  public static (AppDbContext Db, SqliteConnection Conn) NewRelational()
  {
    var conn = new SqliteConnection("DataSource=:memory:");
    conn.Open(); // Garder ouvert pendant le test

    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(conn)
        .EnableSensitiveDataLogging()
        .Options;

    var db = new AppDbContext(options);
    db.Database.EnsureCreated(); 

    return (db, conn);
  }
}
