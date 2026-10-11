using Moq;
using RobsSpacePatrol.API.Services;
using RobsSpacePatrol.API.Services.Interfaces;
using RobsSpacePatrol.Factories;
using RobsSpacePatrol.Library;
using RobsSpacePatrol.Library.Factories;
using RobsSpacePatrol.Library.Game;
using RobsSpacePatrol.Library.Models;

namespace RobsSpacePatrol.API.Tests;

public class Builder
{
    public Game getGame()
    {
        var randomFactory = new RandomFactory();
        var galaxyFactory = new GalaxyFactory();
        var currentQuadrantFactory = new CurrentQuadrantFactory();
        var quadrantFactory = new QuadrantFactory();

        var galaxy = galaxyFactory.createGalaxy(randomFactory, currentQuadrantFactory, quadrantFactory);
        var game = new Game(galaxy, randomFactory, currentQuadrantFactory);

        return game;
    }

    public IGameDBService GetGameService()
    {
        var moq = new Mock<IGameDBService>();
        var gameMock = getGame();
        moq.Setup(z => z.CreateGame(It.IsAny<Galaxy>())).Returns(gameMock);
        moq.Setup(z => z.GetGame(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(gameMock);
        moq.Setup(x => x.SaveGame(It.IsAny<Guid>(), It.IsAny<IGame>(), It.IsAny<string>())).ReturnsAsync(SaveGameResult.Saved);
        return moq.Object;
    }

    public static IGame Game
    {
        get
        {
            return new Builder().getGame();
        }
    }

    public static IGameDBService GameService
    {
        get
        {
            return new Builder().GetGameService();
        }
    }
}
