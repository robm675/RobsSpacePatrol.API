using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public class LRS_Tests
{
    [Fact]
    public async Task Test_LRS_Contents()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, new CoordDTO(0, 0));
        await DebugMethods.SetQuadrant(data, new(0, 0), new(new(0, 0), 3, 5, true, false));
        await DebugMethods.SetQuadrant(data, new(1, 0), new(new(1, 0), 0, 1, true, true));
        await DebugMethods.SetQuadrant(data, new(0, 1), new(new(0, 1), 0, 1, true, true));
        await DebugMethods.SetQuadrant(data, new(1, 1), new(new(1, 1), 4, 9, true, false));

        var response = await DebugMethods.PostApi<LRS_Request, LRS_Response>(
            data.client,
            "api/game/lrs",
            new LRS_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.quadrants);
        Assert.Equal(4, response.Data.quadrants.Count);

        var quad1 = response.Data.quadrants.Single(z => z.Coord == new CoordDTO(0, 0));
        Assert.Equal(new QuadrantDTO(new(0, 0), 3, 5, true, false), quad1);

        var quad2 = response.Data.quadrants.Single(z => z.Coord == new CoordDTO(1, 0));
        Assert.Equal(new QuadrantDTO(new(1, 0), 0, 1, true, true), quad2);

        var quad3 = response.Data.quadrants.Single(z => z.Coord == new CoordDTO(0, 1));
        Assert.Equal(new QuadrantDTO(new(0, 1), 0, 1, true, true), quad3);

        var quad4 = response.Data.quadrants.Single(z => z.Coord == new CoordDTO(1, 1));
        Assert.Equal(new QuadrantDTO(new(1, 1), 4, 9, true, false), quad4);
    }
    [Fact]
    public async Task Test_LRS_Damaged()
    {
        var data = await GameInfo.GetGameKey();

        await DebugMethods.Set_ONE_DeviceDamage(data, eDevice.LRS, -3);
        var response = await DebugMethods.PostApi<LRS_Request, LRS_Response>(
            data.client,
            "api/game/lrs",
            new LRS_Request(data.gameKey));

        Assert.NotNull(response);
        Assert.Null(response.Data);
        Assert.True(response.Damaged);
    }
}
