using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;
public class NAV_Tests
{
    [Fact]
    public async Task NAV_Working_VerifyLocation_EW()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);


        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 1, 2));


        var result = await DebugMethods.GetStarshipQuadrant(data);
        Assert.NotNull(result);
        Assert.Equal(2, result.X);
        Assert.Equal(0, result.Y);
    }
    [Fact]
    public async Task NAV_Working_VerifyLocation_NS()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);


        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 7, 2));


        var result = await DebugMethods.GetStarshipQuadrant(data);
        Assert.NotNull(result);
        Assert.Equal(0, result.X);
        Assert.Equal(2, result.Y);
    }
    [Fact]
    public async Task NAV_Damaged_DistTooLarge()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.NAV, -3);
        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);


        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 1, 2));


        var result = await DebugMethods.GetStarshipQuadrant(data);
        Assert.NotNull(response);
        Assert.True(response.Damaged);
    }
    [Fact]
    public async Task NAV_Damaged_DistOk()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.NAV, -3);
        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);


        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 1, 0.2));


        var result = await DebugMethods.GetSectorsByContent(data, Global.SectorStarship);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].Coord.X);
        Assert.Equal(0, result[0].Coord.Y);
    }
    [Fact]
    public async Task NAV_OutsideGalaxy()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);


        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 5, 1));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(eNavMessage.BadInput_OutsideGalaxy, response.Data.navMessage);
    }
    [Fact]
    public async Task NAV_InsufficientEnergy_ShieldEnergyAvailable()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetStarship(data, new StarshipDTO() { EnergyLevel = 0, ShieldLevel = 2000 });

        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 1, 1));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(eNavMessage.InsufficientEnergy_ShieldEnergyAvailable, response.Data.navMessage);
    }
    [Fact]
    public async Task NAV_InsufficientEnergy_NoShieldEnergy()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.ClearAllCQ(data, true);
        await DebugMethods.SetSectorContents(data, new CoordDTO(0, 0), Global.SectorStarship);
        await DebugMethods.SetStarship(data, new StarshipDTO() { EnergyLevel = 0, ShieldLevel = 0 });

        var response = await DebugMethods.PostApi<NAV_Request, NAV_Response>(
                data.client,
                "api/game/nav",
                new NAV_Request(data.gameKey, 1, 1));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(eNavMessage.InsufficientEnergy, response.Data.navMessage);
    }
}
