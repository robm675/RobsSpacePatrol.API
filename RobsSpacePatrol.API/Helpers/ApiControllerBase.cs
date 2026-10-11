using Microsoft.AspNetCore.Mvc;
using RobsSpacePatrol.API.Services;
using RobsSpacePatrol.DTO;

namespace RobsSpacePatrol.API.Helpers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult? GetSaveFailureResult(SaveGameResult result)
    {
        return result switch
        {
            SaveGameResult.Saved => null,

            SaveGameResult.ConcurrencyConflict => Conflict(ApiResponse<object>.Conflict("The game was changed by another request. Please retry.")),

            SaveGameResult.NotOwner => NotFound(ApiResponse<object>.NotFound("Game not found.")),

            _ => StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, "The game could not be saved."))
        };
    }
}