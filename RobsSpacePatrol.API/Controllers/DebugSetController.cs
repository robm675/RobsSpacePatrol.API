using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobsSpacePatrol.API.Helpers;
using RobsSpacePatrol.API.Services.Interfaces;
using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Debug.Get;
using RobsSpacePatrol.DTO.Debug.Set;
using RobsSpacePatrol.DTO.Library;
using RobsSpacePatrol.Library;
using RobsSpacePatrol.Library.Models;
using System.Security.Claims;

namespace RobsSpacePatrol.API.Controllers;

[Route("api/debug/set")]
[Authorize(Roles = "Admin")]
[ApiController]
public class DebugSetController(IGameDBService gameService) : ApiControllerBase
{
    [HttpPost("sector_contents")]
    public async Task<ActionResult<ApiResponse<bool>>> SetSectorContents(DebugSetSectorContentsRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<bool>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var sector = game.Galaxy.CurrentQuadrant.GetSectorByCoord(Coord.fromDTO(request.Coord));
        if (sector == null)
            return BadRequest(ApiResponse<bool>.BadRequest($"Sector not found"));

        sector.SectorContents = request.contents;

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;


        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("enemy")]
    public async Task<ActionResult<ApiResponse<bool>>> SetEnemy(DebugSetEnemyRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var sector = game.Galaxy.CurrentQuadrant.GetSectorByCoord(Coord.fromDTO(request.Coord));
        if (sector == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Sector not found"));

        sector.SetEnemy(request.enemy.ShieldLevel);

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("quadrant")]
    public async Task<ActionResult<ApiResponse<bool>>> SetQuadrant(DebugSetQuadrantRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var quad = game.Galaxy.GetQuadrantByCoord(Coord.fromDTO(request.Coord));
        if (quad == null)
            return BadRequest(ApiResponse<object>.BadRequest($"quadrant not found"));

        quad.NumberOfStars = request.quadrant.NumberOfStars;
        quad.NumberOfEnemies = request.quadrant.NumberOfEnemies;
        quad.HasBeenExplored = request.quadrant.HasBeenExplored;
        quad.HasStarBase = request.quadrant.HasStarBase;

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("starship")]
    public async Task<ActionResult<ApiResponse<bool>>> SetStarship(DebugSetStarshipRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        game.Galaxy.Starship.ShieldLevel = request.starship.ShieldLevel;
        game.Galaxy.Starship.EnergyLevel = request.starship.EnergyLevel;
        game.Galaxy.Starship.TorpedosRemaining = request.starship.TorpedosRemaining;
        game.Galaxy.Starship.isDocked = request.starship.isDocked;
        game.Galaxy.Starship.isDestroyed = request.starship.isDestroyed;
        foreach (var dev in request.starship.Devices)
        {
            var devToChange = game.Galaxy.Starship.Devices.SingleOrDefault(z => z.Name == dev.Name);
            if (devToChange == null)
                return BadRequest(ApiResponse<object>.BadRequest($"Device not found={dev.Name}"));

            devToChange.DamageLevel = dev.DamageLevel;
        }

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("galaxy")]
    public async Task<ActionResult<ApiResponse<bool>>> SetGalaxy(DebugSetGalaxyRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        if (request.missionTime != -1)
            game.Galaxy.MissionTime = request.missionTime;
        if (request.missionTimeDeadline != -1)
            game.Galaxy.MissionTimeDeadLine = request.missionTimeDeadline;

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("clearcq")]
    public async Task<ActionResult<ApiResponse<bool>>> SetClearCQ(DebugSetClearCurrentQuadrantRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<bool>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        for (int x = 0; x < Global.MaxQuadrantsXY; x++)
        {
            for (int y = 0; y < Global.MaxQuadrantsXY; y++)
            {
                var coord = new Coord(x, y);
                var targetSector = game.Galaxy.CurrentQuadrant.GetSectorByCoord(coord);
                if (targetSector == null)
                    return BadRequest(ApiResponse<bool>.BadRequest($"Sector has not been found={coord.ToString()}"));

                if (request.all)
                    targetSector.SectorContents = Global.SectorEmpty;
                else
                {
                    if (targetSector.SectorContents != Global.SectorStarship)
                        targetSector.SectorContents = Global.SectorEmpty;
                }
            }
        }

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("enemy_fire_move")]
    public async Task<ActionResult<ApiResponse<bool>>> SetEnemyFireMove(DebugSetEnemyFireMoveRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<bool>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        game.Galaxy.debugSet(request.skipEnemyMove, request.forceEnemyMove, request.stopEnemyFire);

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }

    [HttpPost("starship_quadrant")]
    public async Task<ActionResult<ApiResponse<bool>>> SetStarshipQuadrant(DebugSetStarshipQuadrantRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.GameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest($"Game key not found={request.GameKey.ToString()}"));

        var quad = game.Galaxy.GetQuadrantByCoordDTO(request.coord);
        if (quad == null)
            return BadRequest(ApiResponse<object>.BadRequest($"quadrant not found"));

        var targetQuad = game.Galaxy.GetQuadrantByCoordDTO(request.coord);
        var sector = game.Galaxy.CurrentQuadrant.GetSectorByContents(Global.SectorStarship);
        game.Galaxy.MoveStarshipQuadrant(game.CurrentQuadrantFactory, game.RandomFactory, Quadrant.fromDTO(targetQuad!), sector!.Coord);

        var saved = await gameService.SaveGame(request.GameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        return Ok(ApiResponse<bool>.OK(true, "OK"));
    }
}
