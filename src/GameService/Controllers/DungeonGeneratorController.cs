using GameService.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameService.Controllers;

/// <summary>
/// API de génération procédurale de donjons
/// </summary>
[ApiController, Route("api/[controller]")]
public class DungeonGeneratorController : ControllerBase
{
    private readonly DungeonGeneratorService _generator;

    public DungeonGeneratorController(DungeonGeneratorService generator) => _generator = generator;

    [HttpPost("generate")]
    public ActionResult<DungeonGenerationResult> Generate([FromBody] GenerateRequest req)
    {
        var seed = req.Seed ?? Random.Shared.Next();
        var result = _generator.Generate(seed, req.MaxDepth ?? 10, req.MinBranches ?? 1, req.MaxBranches ?? 3);
        return Ok(result);
    }
}

public record GenerateRequest(int? Seed = null, int? MaxDepth = null, int? MinBranches = null, int? MaxBranches = null);
