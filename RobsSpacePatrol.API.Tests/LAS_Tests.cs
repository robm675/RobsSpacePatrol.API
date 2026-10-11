using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;
public class LAS_Tests
{
    [Fact]
    public async Task Test_LAS_Contents()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 6), 100);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 5), 100);
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);



        var response = await DebugMethods.PostApi<LAS_Request, LAS_Response>(
            data.client,
            "api/game/las",
            new LAS_Request(data.gameKey, 300));



        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        var enemy1 = response.Data.objectHitReports.SingleOrDefault(z => z.Coord == new CoordDTO(0, 7));
        var enemy2 = response.Data.objectHitReports.SingleOrDefault(z => z.Coord == new CoordDTO(0, 6));
        var enemy3 = response.Data.objectHitReports.SingleOrDefault(z => z.Coord == new CoordDTO(0, 5));
        Assert.NotNull(enemy1);
        Assert.NotNull(enemy2);
        Assert.NotNull(enemy3);
    }
    [Fact]
    public async Task Test_LAS_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.LAS, -3);
        var response = await DebugMethods.PostApi<LAS_Request, LAS_Response>(
            data.client,
            "api/game/las",
            new LAS_Request(data.gameKey, 1000));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }
    [Fact]
    public async Task Test_LAS_NoEnemies()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);



        var response = await DebugMethods.PostApi<LAS_Request, LAS_Response>(
            data.client,
            "api/game/las",
            new LAS_Request(data.gameKey, 300));



        Assert.NotNull(response);
        Assert.Equal(GlobalVars.NoEnemiesFound, response.Message);
    }
    [Fact]
    public async Task Test_LAS_InsufficientEnergy()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 7), 100);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 6), 100);
        await DebugMethods.SetEnemy(data, new CoordDTO(0, 5), 100);
        await DebugMethods.SetStarship(data, new StarshipDTO() { EnergyLevel = 0 });
        await DebugMethods.SetEnemyFireMove(data, skipEnemyMove: false);



        var response = await DebugMethods.PostApi<LAS_Request, LAS_Response>(
            data.client,
            "api/game/las",
            new LAS_Request(data.gameKey, 300));



        Assert.NotNull(response);
        Assert.Equal(GlobalVars.InsufficientEnergy, response.Message);
    }
}
