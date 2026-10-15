using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobsSpacePatrol.API.DB;
using RobsSpacePatrol.API.Helpers;
using RobsSpacePatrol.API.Services.Interfaces;
using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using RobsSpacePatrol.Factories;
using RobsSpacePatrol.Library.eNums;
using RobsSpacePatrol.Library.Factories;
using RobsSpacePatrol.Library.Factories.Interfaces;
using RobsSpacePatrol.Library.Game;
using RobsSpacePatrol.Library.Models;
using RobsSpacePatrol.Library.Utils;
using System.Diagnostics;
using System.Security.Claims;
using static RobsSpacePatrol.DTO.Library.Result;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RobsSpacePatrol.API.Controllers;

[Route("api/game")]
[Authorize]
[ApiController]
public class GameController(IGameDBService gameService) : ApiControllerBase
{
    [HttpGet("start_game")]
    public async Task<ActionResult<ApiResponse<GameStartResponse>>> GameStart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var game = gameService.CreateGame();
        var json = RobsSpacePatrol.Library.Utils.DB.GetJsonFromGalaxy((Galaxy)game.Galaxy);

        var newGameKey = Guid.NewGuid();
        var saved = await gameService.SaveGame(newGameKey, game, userId);

        var saveFailure = GetSaveFailureResult(saved);
        if (saveFailure != null) return saveFailure;

        var resp = new GameStartResponse(newGameKey);

        return Ok(ApiResponse<GameStartResponse>.OK(resp, "OK"));
    }

    [HttpPost("zap_game")]
    public async Task<ActionResult<ApiResponse<DeleteGameResponse>>> zap_game(ZAP_Game_Request zapRequest)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var deleted = await gameService.DeleteGame(zapRequest.gameKey, userId);

        if (!deleted)
            return NotFound();

        return Ok(ApiResponse<DeleteGameResponse>.OK(new DeleteGameResponse(deleted), "Game Deleted"));
    }

    [HttpPost("srs")]
    public async Task<ActionResult<ApiResponse<SRS_Response>>> srs(SRS_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();



        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.SRS();

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<SRS_Response>.IsDamaged);

            var srs = result.getResults<SRS>().Sectors;
            if (srs == null) return BadRequest(ApiResponse<object>.BadRequest("SRS was null from game."));

            var mrResults = game.RunRoutineMaintenance(0.1);
            var saved = await gameService.SaveGame(request.gameKey, game, userId);

            var saveFailure = GetSaveFailureResult(saved);
            if (saveFailure != null) return saveFailure;

            return Ok(ApiResponse<SRS_Response>.OK(new SRS_Response(srs, mrResults.getResults<MaintResult>()), ""));
        }
    }

    [HttpPost("lrs")]
    public async Task<ActionResult<ApiResponse<LRS_Response>>> lrs(LRS_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();



        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.LRS();

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<SRS_Response>.IsDamaged);

            var lrs = result.getResults<LRS>().Quadrants;

            if (lrs == null) return BadRequest(ApiResponse<object>.BadRequest("LRS was null from game."));

            var mrResults = game.RunRoutineMaintenance(0.1);
            var saved = await gameService.SaveGame(request.gameKey, game, userId);

            var saveFailure = GetSaveFailureResult(saved);
            if (saveFailure != null) return saveFailure;

            var cqCoord = game.Galaxy.CurrentQuadrant.Coord.toDTO;

            return Ok(ApiResponse<LRS_Response>.OK(new LRS_Response(lrs, cqCoord, mrResults.getResults<MaintResult>()), ""));
        }
    }

    [HttpPost("dam")]
    public async Task<ActionResult<ApiResponse<DAM_Response>>> dam(DAM_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();



        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.GetDamageReport();
            var dam = result.getResults<DAM>().Devices;

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<DAM_Response>.IsDamaged);
            else
            {
                if (dam == null) return BadRequest(ApiResponse<object>.BadRequest("DAM was null from game."));
                return Ok(ApiResponse<DAM_Response>.OK(new DAM_Response(dam), ""));
            }
        }
    }

    [HttpPost("com_rec")]
    public async Task<ActionResult<ApiResponse<COM_REC_Response>>> com_rec(COM_REC_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();



        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.COM_REC();

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<COM_REC_Response>.IsDamaged);

            var com_rec = result.getResults<COM_REC>().Quadrants;

            if (com_rec == null) return BadRequest(ApiResponse<object>.BadRequest("COM_REC was null from game."));
            return Ok(ApiResponse<COM_REC_Response>.OK(new COM_REC_Response(com_rec, game.Galaxy.CurrentQuadrant.Coord.toDTO), ""));
        }
    }

    [HttpPost("com_nav")]
    public async Task<ActionResult<ApiResponse<COM_NAV_Response>>> com_nav(COM_NAV_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();



        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.COM_NAV(request.quadrantCoord, request.sectorCoord);

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<COM_NAV_Response>.IsDamaged);

            var com_nav = result.getResults<COM_NAV>();

            if (com_nav == null) return BadRequest(ApiResponse<object>.BadRequest("COM_NAV was null from game."));
            return Ok(ApiResponse<COM_NAV_Response>.OK(new COM_NAV_Response(com_nav.Direction, com_nav.Distance), ""));
        }
    }

    [HttpPost("com_sta")]
    public async Task<ActionResult<ApiResponse<COM_STA_Response>>> com_sta(COM_STA_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.COM_STA();

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<COM_STA_Response>.IsDamaged);

            var com_sta = result.getResults<COM_STA>();

            if (com_sta == null) return BadRequest(ApiResponse<object>.BadRequest("COM_STA was null from game."));

            var allDevices = game.Galaxy.Starship.toDeviceDTO;

            var response = new COM_STA_Response(
                allDevices,
                com_sta.MissionTime,
                com_sta.QuadrantCoord,
                com_sta.SectorCoord,
                com_sta.EnemiesRemaining,
                com_sta.StarBaseRemaining,
                com_sta.EnergyRemaining,
                com_sta.ShieldLevel,
                com_sta.TorpedosRemaining,
                com_sta.Docked,
                com_sta.MissionTimeDeadLine
                );

            return Ok(ApiResponse<COM_STA_Response>.OK(response, ""));
        }
    }

    [HttpPost("com_stb")]
    public async Task<ActionResult<ApiResponse<COM_STB_Response>>> com_stb(COM_STB_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.COM_STB();
            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<COM_STB_Response>.IsDamaged);

            if (result.eCommandResult == eCommandResult.CPU_STB_No_Starbase_Present)
                return Ok(ApiResponse<COM_STB_Response>.Info(GlobalVars.StarbaseNotFound));

            var com_stb = result.getResults<COM_STB>();

            if (com_stb == null) return BadRequest(ApiResponse<object>.BadRequest("COM_STB was null from game."));
            return Ok(ApiResponse<COM_STB_Response>.OK(new COM_STB_Response(com_stb.StarBaseCoord, com_stb.Direction, com_stb.Distance), ""));
        }
    }

    [HttpPost("com_tor")]
    public async Task<ActionResult<ApiResponse<COM_TOR_Response>>> com_tor(COM_TOR_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.COM_TOR();

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<COM_TOR_Response>.IsDamaged);

            if (result.eCommandResult == eCommandResult.No_Enemies_Present)
                return Ok(ApiResponse<COM_TOR_Response>.Info(GlobalVars.NoEnemiesFound));

            var com_tor = result.getResults<COM_TOR>();

            if (com_tor == null) return BadRequest(ApiResponse<object>.BadRequest("COM_TOR was null from game."));
            return Ok(ApiResponse<COM_TOR_Response>.OK(new COM_TOR_Response(com_tor.DirectionToEnemies), ""));
        }
    }

    [HttpPost("nav")]
    public async Task<ActionResult<ApiResponse<NAV_Response>>> nav(NAV_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var startCoord = game.Galaxy.CurrentQuadrant.Coord.toDTO;
            var result = game.NAV(request.dir, request.dist);
            var finalCoord = game.Galaxy.CurrentQuadrant.Coord.toDTO;

            if (result.eCommandResult == eCommandResult.Damaged && request.dist > 0.2)
                return Ok(ApiResponse<NAV_Response>.IsDamaged);

            var nav = result.getResults<NAV>();

            if (nav == null) return BadRequest(ApiResponse<object>.BadRequest("NAV was null from game."));

            var missionTimeChanged = CalculationUtilitiesV2.Distance2Time(nav.DistanceTraveled);
            var mrResults = game.RunRoutineMaintenance(missionTimeChanged);
            var saved = await gameService.SaveGame(request.gameKey, game, userId);

            var saveFailure = GetSaveFailureResult(saved);
            if (saveFailure != null) return saveFailure;

            var ssSectCoord = game.Galaxy.CurrentQuadrant.GetSectorByContents(Global.SectorStarship)?.Coord.toDTO;
            if (ssSectCoord == null) return BadRequest(ApiResponse<object>.BadRequest("sstSector was null from game."));

            return Ok(ApiResponse<NAV_Response>.OK(new NAV_Response(nav.NavMessage, nav.DistanceTraveled, startCoord, finalCoord, ssSectCoord, mrResults.getResults<MaintResult>()), ""));
        }
    }

    [HttpPost("las")]
    public async Task<ActionResult<ApiResponse<LAS_Response>>> las(LAS_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();



        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.LAS(request.energy);

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<LAS_Response>.IsDamaged);

            if (result.eCommandResult == eCommandResult.No_Enemies_Present)
                return Ok(ApiResponse<LAS_Response>.Info(GlobalVars.NoEnemiesFound));

            if (result.eCommandResult == eCommandResult.InsufficientInventory)
                return Ok(ApiResponse<LAS_Response>.Info(GlobalVars.InsufficientEnergy));

            var las = result.getResults<LAS>();

            if (las == null) return BadRequest(ApiResponse<object>.BadRequest("LAS was null from game."));

            var mrResults = game.RunRoutineMaintenance(0.1);
            var saved = await gameService.SaveGame(request.gameKey, game, userId);

            var saveFailure = GetSaveFailureResult(saved);
            if (saveFailure != null) return saveFailure;

            return Ok(ApiResponse<LAS_Response>.OK(new LAS_Response(las.ObjectHitReport, mrResults.getResults<MaintResult>()), ""));
        }
    }

    [HttpPost("tor")]
    public async Task<ActionResult<ApiResponse<TOR_Response>>> tor(TOR_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.TOR(request.dir);

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<TOR_Response>.IsDamaged);

            if (result.eCommandResult == eCommandResult.No_Enemies_Present)
                return Ok(ApiResponse<TOR_Response>.Info(GlobalVars.NoEnemiesFound));

            if (result.eCommandResult == eCommandResult.InsufficientInventory)
                return Ok(ApiResponse<TOR_Response>.Info(GlobalVars.InsufficientInventory));

            if (result.eCommandResult == eCommandResult.TOR_Missed)
            {
                var savedMissed = await gameService.SaveGame(request.gameKey, game, userId);

                var saveFailureMissed = GetSaveFailureResult(savedMissed);
                if (saveFailureMissed != null)
                    return saveFailureMissed;

                return Ok(ApiResponse<TOR_Response>.Info(GlobalVars.TorpMissed));
            }

            var tor = result.getResults<TOR>();

            if (tor == null) return BadRequest(ApiResponse<object>.BadRequest("TOR was null from game."));

            var mrResults = game.RunRoutineMaintenance(0.1);
            var saved = await gameService.SaveGame(request.gameKey, game, userId);

            var saveFailure = GetSaveFailureResult(saved);
            if (saveFailure != null) return saveFailure;

            var enemyExist = game.Galaxy.CurrentQuadrant.Sectors.Any(z => z.HasEnemy);
            var ssShieldLevel = game.Galaxy.Starship.ShieldLevel;

            return Ok(ApiResponse<TOR_Response>.OK(new TOR_Response(tor.ObjectHitReport, enemyExist, ssShieldLevel, mrResults.getResults<MaintResult>()), ""));
        }
    }

    [HttpPost("she")]
    public async Task<ActionResult<ApiResponse<SHE_Response>>> she(SHE_Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();


        var game = await gameService.GetGame(request.gameKey, userId);
        if (game == null)
            return BadRequest(ApiResponse<object>.BadRequest(GlobalVars.GameKeyNotFound));
        else
        {
            var result = game.SHE(request.energy);

            if (result.eCommandResult == eCommandResult.Damaged)
                return Ok(ApiResponse<SHE_Response>.IsDamaged);

            if (result.eCommandResult == eCommandResult.Error)
                return Ok(ApiResponse<SHE_Response>.BadRequest(Global.SHE_ErrorMessage));

            var she = result.getResults<SHE>();

            var mrResults = game.RunRoutineMaintenance(0.1);
            var saved = await gameService.SaveGame(request.gameKey, game, userId);

            var saveFailure = GetSaveFailureResult(saved);
            if (saveFailure != null) return saveFailure;

            if (she == null) return BadRequest(ApiResponse<object>.BadRequest("SHE was null from game."));
            return Ok(ApiResponse<SHE_Response>.OK(new SHE_Response(she.ErrorMessage, mrResults.getResults<MaintResult>()), ""));
        }
    }
}