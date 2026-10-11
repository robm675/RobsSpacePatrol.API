using Microsoft.EntityFrameworkCore;
using RobsSpacePatrol.API.DB;
using RobsSpacePatrol.API.Services.Interfaces;
using RobsSpacePatrol.Factories;
using RobsSpacePatrol.Library;
using RobsSpacePatrol.Library.Factories;
using RobsSpacePatrol.Library.Factories.Interfaces;
using RobsSpacePatrol.Library.Game;
using RobsSpacePatrol.Library.Models;

namespace RobsSpacePatrol.API.Services;

public enum SaveGameResult
{
    Saved,
    NotFound,
    NotOwner,
    ConcurrencyConflict,
    Error
}

public class GameDBService(ApplicationDbContext context, ILogger<GameDBService> logger) : IGameDBService
{
    public async Task<IGame?> GetGame(Guid gameKey, string userId)
    {
        var rec = await context.SaveGames.SingleOrDefaultAsync(z => z.Key == gameKey && z.UserId == userId);

        if (rec == null)
        {
            return null;
        }

        var galaxy = Library.Utils.DB.GetGalaxyFromJson(rec.JSON);

        return CreateGame(galaxy);
    }

    public async Task<SaveGameResult> SaveGame(Guid gameKey, IGame game, string userId)
    {
        try
        {
            var record = await context.SaveGames.SingleOrDefaultAsync(x => x.Key == gameKey);

            var json = Library.Utils.DB.GetJsonFromGalaxy((Galaxy)game.Galaxy);
            if (record == null)
            {
                record = new SaveGame
                {
                    Key = gameKey,
                    JSON = json,
                    UserId = userId
                };

                await context.SaveGames.AddAsync(record);
            }
            else
            {
                if (record.UserId != userId)
                    return SaveGameResult.NotOwner;

                record.JSON = json;
            }
            await context.SaveChangesAsync();

            return SaveGameResult.Saved;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(ex, "Concurrency conflict while saving game {GameKey}", gameKey);
            return SaveGameResult.ConcurrencyConflict;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while saving game {GameKey}", gameKey);
            return SaveGameResult.Error;
        }
    }

    public IGame CreateGame(IGalaxy? galaxy = null)
    {
        IRandomFactory randomFactory = new RandomFactory();

        var currentQuadrantFactory = new CurrentQuadrantFactory();

        if (galaxy == null)
            galaxy = new GalaxyFactory().createGalaxy(randomFactory, currentQuadrantFactory, new QuadrantFactory());

        var game = new Game(galaxy, randomFactory, currentQuadrantFactory);

        return game;
    }

    public async Task<bool> DeleteGame(Guid gameKey, string userId)
    {
        var game = context.SaveGames.FirstOrDefault(z => z.Key == gameKey && z.UserId == userId);
        if (game == null) return false;

        context.SaveGames.Remove(game);
        await context.SaveChangesAsync();
        return true;
    }
}
