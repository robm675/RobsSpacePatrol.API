using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class COM_REC_Tests
{
    [Fact]
    public async Task Test_COM_REC_Contents()
    {
        var data = await GameInfo.GetGameKey();

        var response = await DebugMethods.PostApi<COM_REC_Request, COM_REC_Response>(
            data.client,
            "api/game/com_rec",
            new COM_REC_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.quadrants);
        Assert.Single(response.Data.quadrants);
    }
    [Fact]
    public async Task Test_COM_REC_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.COM, -3);
        var response = await DebugMethods.PostApi<COM_REC_Request, COM_REC_Response>(
            data.client,
            "api/game/com_rec",
            new COM_REC_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }
}
