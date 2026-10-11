using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;
public class COM_NAV_Tests
{
    [Fact]
    public async Task Test_COM_NAV_Contents()
    {
        var data = await GameInfo.GetGameKey();

        var response = await DebugMethods.PostApi<COM_NAV_Request, COM_NAV_Response>(
            data.client,
            "api/game/com_nav",
            new COM_NAV_Request(data.gameKey, new CoordDTO(1,2), new CoordDTO(3,4)));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEqual(0, response.Data.dir);
        Assert.NotEqual(0, response.Data.dist);
    }
    [Fact]
    public async Task Test_COM_NAV_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.COM, -3);
        var response = await DebugMethods.PostApi<COM_NAV_Request, COM_NAV_Response>(
            data.client,
            "api/game/com_nav",
            new COM_NAV_Request(data.gameKey, new CoordDTO(1, 2), new CoordDTO(3, 4)));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }

}
