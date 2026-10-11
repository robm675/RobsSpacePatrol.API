using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class COM_STB_Tests
{
    [Fact]
    public async Task Test_COM_STB_Contents()
    {
        var data = await GameInfo.GetGameKey();
        var ssCoord = new CoordDTO(1, 1);
        var sbCoord = new CoordDTO(5, 1);

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, sbCoord, Global.SectorStarBase);
        await DebugMethods.SetSectorContents(data, ssCoord, Global.SectorStarship);
        var response = await DebugMethods.PostApi<COM_STB_Request, COM_STB_Response>(
            data.client,
            "api/game/com_stb",
            new COM_STB_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(4, response.Data.dist);
        Assert.Equal(1, response.Data.dir);
        Assert.Equal(sbCoord, response.Data.starbaseCoord);
    }

    [Fact]
    public async Task Test_COM_STB_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.COM, -3);
        var response = await DebugMethods.PostApi<COM_STB_Request, COM_STB_Response>(
            data.client,
            "api/game/com_stb",
            new COM_STB_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }

    [Fact]
    public async Task Test_COM_STB_NoStarbase()
    {
        var data = await GameInfo.GetGameKey();
        var ssCoord = new CoordDTO(1, 1);

        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, ssCoord, Global.SectorStarship);
        var response = await DebugMethods.PostApi<COM_STB_Request, COM_STB_Response>(
            data.client,
            "api/game/com_stb",
            new COM_STB_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.True(response.InfoOnly);
        Assert.Equal(GlobalVars.StarbaseNotFound, response.Message);
    }
}