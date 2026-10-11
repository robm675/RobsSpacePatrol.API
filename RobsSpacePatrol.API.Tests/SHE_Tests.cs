using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;
public class SHE_Tests
{
    [Fact]
    public async Task Test_SHE_Contents()
    {
        var data = await GameInfo.GetGameKey();



        var response = await DebugMethods.PostApi<SHE_Request, SHE_Response>(
            data.client,
            "api/game/she",
            new SHE_Request(data.gameKey, 100));



        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data.message);
    }
    [Fact]
    public async Task Test_SHE_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.SHE, -3);
        var response = await DebugMethods.PostApi<SHE_Request, SHE_Response>(
            data.client,
            "api/game/she",
            new SHE_Request(data.gameKey, 7));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }
    [Fact]
    public async Task Test_SHE_InvalidAmount()
    {
        var data = await GameInfo.GetGameKey();



        var response = await DebugMethods.PostApi<SHE_Request, SHE_Response>(
            data.client,
            "api/game/she",
            new SHE_Request(data.gameKey, 4000));



        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.Equal(Global.SHE_ErrorMessage, response.Message);
    }
}
