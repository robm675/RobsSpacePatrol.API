using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class TOR_Tests
{
    [Fact]
    public async Task Test_TOR_Contents()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);
        var before = await DebugMethods.GetStarship(data);



        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 7));
        var after = await DebugMethods.GetStarship(data);



        Assert.NotNull(before);
        Assert.NotNull(after);
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(new CoordDTO(0, 7), response.Data.objectHitReport.Coord);
        Assert.True(response.Data.objectHitReport.Destroyed);
        Assert.Equal(eObjectHit.Enemy, response.Data.objectHitReport.ObjectHit);
        Assert.Equal(before.TorpedosRemaining - 1, after.TorpedosRemaining);
    }

    [Fact]
    public async Task Test_TOR_CheckDIR6()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(1, 1), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 2), 100);
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);



        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 6));



        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(new CoordDTO(0, 2), response.Data.objectHitReport.Coord);
        Assert.True(response.Data.objectHitReport.Destroyed);
        Assert.Equal(eObjectHit.Enemy, response.Data.objectHitReport.ObjectHit);
    }

    [Fact]
    public async Task Test_TOR_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.TOR, -3);
        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 7));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }

    [Fact]
    public async Task Test_TOR_NoEnemies()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);



        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 3));



        Assert.NotNull(response);
        Assert.Equal(GlobalVars.NoEnemiesFound, response.Message);
    }

    [Fact]
    public async Task Test_TOR_InsufficientInventory()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetStarship(data, new StarshipDTO() { TorpedosRemaining = 0 });
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);



        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 3));



        Assert.NotNull(response);
        Assert.Equal(GlobalVars.InsufficientInventory, response.Message);
    }

    [Fact]
    public async Task Test_TOR_Missed()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);
        var before = await DebugMethods.GetStarship(data);



        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 1));
        var after = await DebugMethods.GetStarship(data);


        Assert.NotNull(before);
        Assert.NotNull(response);
        Assert.NotNull(after);
        Assert.Equal(GlobalVars.TorpMissed, response.Message);
        Assert.Equal(before.TorpedosRemaining - 1, after.TorpedosRemaining);
    }

    [Fact]
    public async Task Test_TOR_StarbaseDestroyed()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetSectorContents(data, new CoordDTO(3, 0), Global.SectorStarBase);



        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 1));



        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(new CoordDTO(3, 0), response.Data.objectHitReport.Coord);
        Assert.True(response.Data.objectHitReport.Destroyed);
        Assert.Equal(eObjectHit.Starbase, response.Data.objectHitReport.ObjectHit);
    }
}