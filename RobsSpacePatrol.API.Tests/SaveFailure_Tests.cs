using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RobsSpacePatrol.API.Controllers;
using RobsSpacePatrol.API.Services;
using RobsSpacePatrol.API.Services.Interfaces;
using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Debug.Set;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using RobsSpacePatrol.Library;
using RobsSpacePatrol.Library.Models;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class SaveFailure_Tests
{
    private const string UserId = "test-user";

    [Fact]
    public async Task GameStart_SaveFails_ReturnsInternalServerError()
    {
        var game = new Builder().getGame();
        var gameService = new Mock<IGameDBService>();
        gameService
            .Setup(x => x.CreateGame(It.IsAny<IGalaxy?>()))
            .Returns(game);
        gameService
            .Setup(x => x.SaveGame(It.IsAny<Guid>(), game, UserId))
            .ReturnsAsync(SaveGameResult.Error);
        var controller = CreateGameController(gameService.Object);

        var response = await controller.GameStart();

        var error = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
        gameService.Verify(
            x => x.SaveGame(It.IsAny<Guid>(), game, UserId),
            Times.Once);
    }

    [Fact]
    public async Task Srs_SaveFails_ReturnsInternalServerError()
    {
        var gameKey = Guid.NewGuid();
        var game = new Builder().getGame();
        var gameService = CreateServiceWithExistingGame(gameKey, game);
        var controller = CreateGameController(gameService.Object);

        var response = await controller.srs(new SRS_Request(gameKey));

        var error = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
        gameService.Verify(
            x => x.SaveGame(gameKey, game, UserId),
            Times.Once);
    }

    [Theory]
    [InlineData(SaveGameResult.ConcurrencyConflict, StatusCodes.Status409Conflict)]
    [InlineData(SaveGameResult.NotOwner, StatusCodes.Status404NotFound)]
    public async Task Srs_SaveRejected_ReturnsMatchingHttpAndBodyStatus(
        SaveGameResult saveResult,
        int expectedStatusCode)
    {
        var gameKey = Guid.NewGuid();
        var game = new Builder().getGame();
        var gameService = CreateServiceWithExistingGame(gameKey, game, saveResult);
        var controller = CreateGameController(gameService.Object);

        var response = await controller.srs(new SRS_Request(gameKey));

        var error = Assert.IsAssignableFrom<ObjectResult>(response.Result);
        Assert.Equal(expectedStatusCode, error.StatusCode);
        var body = Assert.IsType<ApiResponse<object>>(error.Value);
        Assert.Equal(expectedStatusCode, body.StatusCode);
        Assert.False(body.Success);
        gameService.Verify(
            x => x.SaveGame(gameKey, game, UserId),
            Times.Once);
    }

    [Fact]
    public async Task SetSectorContents_SaveFails_ReturnsInternalServerError()
    {
        var gameKey = Guid.NewGuid();
        var game = new Builder().getGame();
        var gameService = CreateServiceWithExistingGame(gameKey, game);
        var controller = new DebugSetController(gameService.Object)
        {
            ControllerContext = CreateControllerContext()
        };
        var request = new DebugSetSectorContentsRequest(
            gameKey,
            new CoordDTO(0, 0),
            Global.SectorEmpty);

        var response = await controller.SetSectorContents(request);

        var error = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
        gameService.Verify(
            x => x.SaveGame(gameKey, game, UserId),
            Times.Once);
    }

    private static Mock<IGameDBService> CreateServiceWithExistingGame(
        Guid gameKey,
        IGame game,
        SaveGameResult saveResult = SaveGameResult.Error)
    {
        var gameService = new Mock<IGameDBService>();
        gameService
            .Setup(x => x.GetGame(gameKey, UserId))
            .ReturnsAsync(game);
        gameService
            .Setup(x => x.SaveGame(gameKey, game, UserId))
            .ReturnsAsync(saveResult);
        return gameService;
    }

    private static GameController CreateGameController(IGameDBService gameService)
    {
        return new GameController(gameService)
        {
            ControllerContext = CreateControllerContext()
        };
    }

    private static ControllerContext CreateControllerContext()
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, UserId)],
                        "TestAuthentication"))
            }
        };
    }
}
