using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;


public class COM_STA_Tests
{
    [Fact]
    public async Task Test_COM_STA_Contents()
    {
        var data = await GameInfo.GetGameKey();

        var response = await DebugMethods.PostApi<COM_STA_Request, COM_STA_Response>(
            data.client,
            "api/game/com_sta",
            new COM_STA_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEqual(0, response.Data.missionTime);
        Assert.NotEqual(0, response.Data.starBaseRemaining);
        Assert.NotEqual(0, response.Data.enemiesRemaining);
        Assert.NotEqual(0, response.Data.energyRemaining);
        Assert.NotEqual(0, response.Data.torpedosRemaining);
    }

    [Fact]
    public async Task Test_COM_STA_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.COM, -3);
        var response = await DebugMethods.PostApi<COM_STA_Request, COM_STA_Response>(
            data.client,
            "api/game/com_sta",
            new COM_STA_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }

}