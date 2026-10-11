using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobsSpacePatrol.API.Services.Interfaces;
using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Debug.Get;
using RobsSpacePatrol.DTO.Debug.Set;
using RobsSpacePatrol.Library;
using System.Security.Claims;
using System.Xml.Linq;


namespace RobsSpacePatrol.API.Controllers;

[Route("api/debug/get")]
[Authorize(Roles = "Admin")]
[ApiController]
public class DebugGetController(IGameDBService gameService) : ControllerBase
{
    [HttpGet("check")]
    public ActionResult<ApiResponse<bool>> Check()
    {
        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpGet("check_game")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckGame(Guid gameKey)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest("Game key not found"));
        else
            return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("sector_contents")]
    public async Task<ActionResult<ApiResponse<DebugGetSectorContentsResponse>>> GetSectorContents(DebugGetSectorContentsRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var sector = game.Galaxy.CurrentQuadrant.GetSectorByCoordDTO(request.Coord);
        if (sector == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Sector not found"));

        var resp = new DebugGetSectorContentsResponse(sector.SectorContents);

        return Ok(ApiResponse<DebugGetSectorContentsResponse>.OK(resp, "OK"));
    }

    [HttpPost("sectors_by_content")]
    public async Task<ActionResult<ApiResponse<DebugGetSectorsByContentResponse>>> GetSectorsByContent(DebugGetSectorsByContentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var matchingSectors = game.Galaxy.CurrentQuadrant.toSectorDTO.Where(z => z.SectorContents.ToUpper().Trim() == request.contents.ToUpper().Trim()).ToList();
        var resp = new DebugGetSectorsByContentResponse(matchingSectors);

        return Ok(ApiResponse<DebugGetSectorsByContentResponse>.OK(resp, "OK"));
    }

    [HttpPost("enemy")]
    public async Task<ActionResult<ApiResponse<DebugGetEnemyResponse>>> GetEnemy(DebugGetEnemyRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var sector = game.Galaxy.CurrentQuadrant.GetSectorByCoordDTO(request.Coord);
        if (sector == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Sector not found"));

        var resp = new DebugGetEnemyResponse(sector.Enemy);

        return Ok(ApiResponse<DebugGetEnemyResponse>.OK(resp, "OK"));
    }

    [HttpPost("quadrant")]
    public async Task<ActionResult<ApiResponse<DebugGetQuadrantResponse>>> GetQuadrant(DebugGetQuadrantRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var quad = game.Galaxy.GetQuadrantByCoordDTO(request.Coord);
        if (quad == null)
            return BadRequest(ApiResponse<object>.BadRequest($"quadrant not found"));

        var resp = new DebugGetQuadrantResponse(quad);

        return Ok(ApiResponse<DebugGetQuadrantResponse>.OK(resp, "OK"));
    }

    [HttpPost("starship")]
    public async Task<ActionResult<ApiResponse<DebugGetStarshipResponse>>> GetStarship(DebugGetStarshipRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var resp = new DebugGetStarshipResponse(game.Galaxy.Starship.toDTO);

        return Ok(ApiResponse<DebugGetStarshipResponse>.OK(resp, "OK"));
    }

    [HttpPost("galaxy")]
    public async Task<ActionResult<ApiResponse<DebugGetGalaxyResponse>>> GetGalaxy(DebugGetGalaxyRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);

        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var resp = new DebugGetGalaxyResponse(game.Galaxy.MissionTime, game.Galaxy.MissionTimeDeadLine);

        return Ok(ApiResponse<DebugGetGalaxyResponse>.OK(resp, "OK"));
    }

    [HttpPost("starship_quadrant")]
    public async Task<ActionResult<ApiResponse<DebugGetStarshipQuadrantResponse>>> GetStarshipQuadrant(DebugGetStarshipQuadrantRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var curQCoord = game.Galaxy.CurrentQuadrant.Coord;

        var resp = new DebugGetStarshipQuadrantResponse(curQCoord.toDTO);

        return Ok(ApiResponse<DebugGetStarshipQuadrantResponse>.OK(resp, "OK"));
    }
}
