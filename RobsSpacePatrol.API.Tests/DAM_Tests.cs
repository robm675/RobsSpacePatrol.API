using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;
public class DAM_Tests
{
    [Fact]
    public async Task Test_DAM_Contents()
    {
        var data = await GameInfo.GetGameKey();
        var starship = new StarshipDTO();
        starship.Devices.First(z => z.Name == "LRS").DamageLevel = -1;
        starship.Devices.First(z => z.Name == "SRS").DamageLevel = -2;
        starship.Devices.First(z => z.Name == "TOR").DamageLevel = -3;
        await DebugMethods.SetStarship(data, starship);
        var response = await DebugMethods.PostApi<DAM_Request, DAM_Response>(
            data.client,
            "api/game/dam",
            new DAM_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.devices);
        Assert.Equal(3, response.Data.devices.Count);
        Assert.Equal(-1, response.Data.devices.Single(z => z.Name == "LRS").DamageLevel);
        Assert.Equal(-2, response.Data.devices.Single(z => z.Name == "SRS").DamageLevel);
        Assert.Equal(-3, response.Data.devices.Single(z => z.Name == "TOR").DamageLevel);
    }
}
