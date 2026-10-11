using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class RoutineMaint_Tests
{
    [Fact]
    public async Task Test_SRS_MissionTimeChanged()
    {
        var data = await GameInfo.GetGameKey();

        var missionTimeBefore = await DebugMethods.GetGalaxy(data);
        var response = await DebugMethods.PostApi<SRS_Request, SRS_Response>(
            data.client,
            "api/game/srs",
            new SRS_Request(data.gameKey));
        var missionTimeAfter = await DebugMethods.GetGalaxy(data);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEqual(missionTimeBefore.missionTime, missionTimeAfter.missionTime);
    }

    [Fact]
    public async Task Test_LRS_MissionTimeChanged()
    {
        var data = await GameInfo.GetGameKey();

        var missionTimeBefore = await DebugMethods.GetGalaxy(data);
        var response = await DebugMethods.PostApi<LRS_Request, LRS_Response>(
            data.client,
            "api/game/lrs",
            new LRS_Request(data.gameKey));
        var missionTimeAfter = await DebugMethods.GetGalaxy(data);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEqual(missionTimeBefore.missionTime, missionTimeAfter.missionTime);
    }

    [Fact]
    public async Task Test_TOR_MissionTimeChanged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);
        var missionTimeBefore = await DebugMethods.GetGalaxy(data);
        var response = await DebugMethods.PostApi<TOR_Request, TOR_Response>(
            data.client,
            "api/game/tor",
            new TOR_Request(data.gameKey, 7));
        var missionTimeAfter = await DebugMethods.GetGalaxy(data);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEqual(missionTimeBefore.missionTime, missionTimeAfter.missionTime);
    }

    [Fact]
    public async Task Test_LAS_MissionTimeChanged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);
        var missionTimeBefore = await DebugMethods.GetGalaxy(data);
        var response = await DebugMethods.PostApi<LAS_Request, LAS_Response>(
            data.client,
            "api/game/las",
            new LAS_Request(data.gameKey, 300));
        var missionTimeAfter = await DebugMethods.GetGalaxy(data);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEqual(missionTimeBefore.missionTime, missionTimeAfter.missionTime);
    }

    [Fact]
    public async Task Test_NAV_MissionTimeChanged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(
            data,
            new CoordDTO(0, 0),
            Global.SectorStarship);
        var missionTimeBefore = await DebugMethods.GetGalaxy(data);
        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 1, 2));
        var missionTimeAfter = await DebugMethods.GetGalaxy(data);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEqual(missionTimeBefore.missionTime, missionTimeAfter.missionTime);
    }

    [Fact]
    public async Task Test_Maint_EnemyFiredCoords()
    {
        var data = await GameInfo.GetGameKey();
        var ssCoord = new CoordDTO(4, 1);
        var enemy1Coord = new CoordDTO(0, 1);
        var enemy2Coord = new CoordDTO(6, 1);
        var enemy3Coord = new CoordDTO(4, 4);

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetEnemy(data, enemy1Coord, 10);
        await DebugMethods.SetEnemy(data, enemy2Coord, 10);
        await DebugMethods.SetEnemy(data, enemy3Coord, 10);
        await DebugMethods.SetSectorContents(data, ssCoord, Global.SectorStarship);
        await DebugMethods.SetEnemyFireMove(data, stopEnemyFire: false);
        var response = await DebugMethods.PostApi<SRS_Request, SRS_Response>(
            data.client,
            "api/game/srs",
            new SRS_Request(data.gameKey));



        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.maintResult);
    }

    [Fact]
    public async Task Test_Maint_Docked()
    {
        var data = await GameInfo.GetGameKey();
        var ssCoord = new CoordDTO(0, 0);
        var sbCoord = new CoordDTO(0, 1);

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, ssCoord, Global.SectorStarship);
        await DebugMethods.SetSectorContents(data, sbCoord, Global.SectorStarBase);
        await DebugMethods.SetEnemyFireMove(data, stopEnemyFire: false);
        var response = await DebugMethods.PostApi<SRS_Request, SRS_Response>(
            data.client,
            "api/game/srs",
            new SRS_Request(data.gameKey));



        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.maintResult);
        Assert.NotNull(response.Data.maintResult.DockingStatus);
        Assert.True(response.Data.maintResult.DockingStatus.CurrentStatus);
    }

    [Fact]
    public async Task Test_Maint_EnemiesMoved()
    {
        var data = await GameInfo.GetGameKey();
        var entCoord = new CoordDTO(0, 0);
        var enemyCoord = new CoordDTO(0, 1);

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, entCoord, Global.SectorStarship);
        await DebugMethods.SetEnemy(data, enemyCoord, 100);
        await DebugMethods.SetEnemyFireMove(data, forceEnemyMove: true, stopEnemyFire: true);


        var response = await DebugMethods.PostApi<SRS_Request, SRS_Response>(
            data.client,
            "api/game/srs",
            new SRS_Request(data.gameKey));



        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.maintResult);
        Assert.NotNull(response.Data.maintResult.EnemiesMoved);
        Assert.Single(response.Data.maintResult.EnemiesMoved);
        Assert.Equal(enemyCoord, response.Data.maintResult.EnemiesMoved[0].EnemyCoord_Orig);
        Assert.NotEqual(enemyCoord, response.Data.maintResult.EnemiesMoved[0].EnemyCoord_Dest);
    }
}