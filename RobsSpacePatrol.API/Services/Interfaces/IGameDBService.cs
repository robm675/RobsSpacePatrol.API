using RobsSpacePatrol.Library;
using RobsSpacePatrol.Library.Game;
using RobsSpacePatrol.Library.Models;

namespace RobsSpacePatrol.API.Services.Interfaces;

public interface IGameDBService
{
    Task<IGame?> GetGame(Guid gameKey, string userId);

    Task<SaveGameResult> SaveGame(Guid gameKey, IGame game, string userId);

    IGame CreateGame(IGalaxy? galaxy = null);

    Task<bool> DeleteGame(Guid gameKey, string userId);
}