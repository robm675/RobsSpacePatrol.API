using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RobsSpacePatrol.API.Controllers;
using RobsSpacePatrol.API.DB;
using RobsSpacePatrol.API.Services;
using RobsSpacePatrol.API.Services.Interfaces;
using RobsSpacePatrol.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class GameOwnership_Tests
{
    [Fact]
    public async Task SaveGame_ExistingKeyOwnedByAnotherUser_IsRejectedWithoutChangingRecord()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var context = CreateContext();
        var service = new GameDBService(context, NullLogger<GameDBService>.Instance);
        var gameKey = Guid.NewGuid();
        const string ownerUserId = "owner-user";
        const string otherUserId = "other-user";

        var ownerGame = service.CreateGame();
        Assert.Equal(SaveGameResult.Saved, await service.SaveGame(gameKey, ownerGame, ownerUserId));

        var originalRecord = await context.SaveGames
            .AsNoTracking()
            .SingleAsync(x => x.Key == gameKey, cancellationToken);

        var otherGame = service.CreateGame();
        otherGame.Galaxy.Starship.EnergyLevel = 1;

        Assert.Equal(SaveGameResult.NotOwner, await service.SaveGame(gameKey, otherGame, otherUserId));

        var storedRecords = await context.SaveGames
            .AsNoTracking()
            .Where(x => x.Key == gameKey)
            .ToListAsync(cancellationToken);

        var storedRecord = Assert.Single(storedRecords);
        Assert.Equal(ownerUserId, storedRecord.UserId);
        Assert.Equal(originalRecord.JSON, storedRecord.JSON);
    }

    [Fact]
    public async Task GetAndDeleteGame_DifferentUser_CannotAccessOwnersGame()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var context = CreateContext();
        var service = new GameDBService(context, NullLogger<GameDBService>.Instance);
        var gameKey = Guid.NewGuid();
        const string ownerUserId = "owner-user";
        const string otherUserId = "other-user";

        Assert.Equal(SaveGameResult.Saved, await service.SaveGame(gameKey, service.CreateGame(), ownerUserId));

        Assert.Null(await service.GetGame(gameKey, otherUserId));
        Assert.False(await service.DeleteGame(gameKey, otherUserId));
        Assert.True(await context.SaveGames.AnyAsync(x => x.Key == gameKey, cancellationToken));

        Assert.NotNull(await service.GetGame(gameKey, ownerUserId));
        Assert.True(await service.DeleteGame(gameKey, ownerUserId));
        Assert.False(await context.SaveGames.AnyAsync(x => x.Key == gameKey, cancellationToken));
    }

    [Fact]
    public async Task CheckGame_MissingOrUnownedGame_ReturnsBadRequest()
    {
        var gameKey = Guid.NewGuid();
        const string userId = "current-user";
        var gameService = new Mock<IGameDBService>();
        gameService
            .Setup(x => x.GetGame(gameKey, userId))
            .ReturnsAsync((IGame?)null);

        var controller = new DebugGetController(gameService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            [new Claim(ClaimTypes.NameIdentifier, userId)],
                            "TestAuthentication"))
                }
            }
        };

        var response = await controller.CheckGame(gameKey);

        Assert.IsType<BadRequestObjectResult>(response.Result);
        gameService.Verify(x => x.GetGame(gameKey, userId), Times.Once);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"game-ownership-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
