using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;


public class SRS_Tests
{
    [Fact]
    public async Task Test_SRS_Contents()
    {
        var data = await GameInfo.GetGameKey();

        var response = await DebugMethods.PostApi<SRS_Request, SRS_Response>(
            data.client,
            "api/game/srs",
            new SRS_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.sectors);
        Assert.Equal(64, response.Data.sectors.Count);
    }
    [Fact]
    public async Task Test_SRS_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.SRS, -3);
        var response = await DebugMethods.PostApi<SRS_Request, SRS_Response>(
            data.client,
            "api/game/srs",
            new SRS_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }
}
