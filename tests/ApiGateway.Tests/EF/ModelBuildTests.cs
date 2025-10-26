using ApiGateway.Data;
using Xunit;

namespace ApiGateway.Tests.EF;

public class ModelBuildTests
{
  [Fact]
  public void AppDbContext_Model_BuildsWithoutError()
  {
    using var db = TestDb.New();
    db.Database.EnsureCreated();
  }
}
