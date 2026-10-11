using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class COM_TOR_Tests
{
    [Fact]
    public async Task Test_COM_TOR_Contents()
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
        var response = await DebugMethods.PostApi<COM_TOR_Request, COM_TOR_Response>(
            data.client,
            "api/game/com_tor",
            new COM_TOR_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        var enemyLoc1 = response.Data.directionToEnemies.Single(z => z.Coord == enemy1Coord);
        var enemyLoc2 = response.Data.directionToEnemies.Single(z => z.Coord == enemy2Coord);
        var enemyLoc3 = response.Data.directionToEnemies.Single(z => z.Coord == enemy3Coord);
        Assert.NotNull(enemyLoc1);
        Assert.NotNull(enemyLoc2);
        Assert.NotNull(enemyLoc3);

        Assert.Equal(5, enemyLoc1.Direction);
        Assert.Equal(1, enemyLoc2.Direction);
        Assert.Equal(7, enemyLoc3.Direction);

        Assert.Equal(4, enemyLoc1.Distance);
        Assert.Equal(2, enemyLoc2.Distance);
        Assert.Equal(3, enemyLoc3.Distance);
    }

    [Fact]
    public async Task Test_COM_TOR_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.COM, -3);
        var response = await DebugMethods.PostApi<COM_TOR_Request, COM_TOR_Response>(
            data.client,
            "api/game/com_tor",
            new COM_TOR_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }

    [Fact]
    public async Task Test_COM_TOR_NoEnemy()
    {
        var data = await GameInfo.GetGameKey();
        var ssCoord = new CoordDTO(1, 1);

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, ssCoord, Global.SectorStarship);
        var response = await DebugMethods.PostApi<COM_TOR_Request, COM_TOR_Response>(
            data.client,
            "api/game/com_tor",
            new COM_TOR_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.True(response.InfoOnly);
        Assert.Equal(GlobalVars.NoEnemiesFound, response.Message);
    }

}