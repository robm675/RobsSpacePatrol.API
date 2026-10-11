using RobsSpacePatrol.DTO.Library;
using Xunit;

[assembly: CaptureConsole]

namespace RobsSpacePatrol.API.Tests;

public class TestDebugAPI
{
    [Fact]
    public async Task StartGame_ReturnsFakeData()
    {
        var data = await GameInfo.GetGameKey();

        Assert.NotNull(data);
        Assert.NotNull(data.client);
        Assert.NotEqual(Guid.Empty, data.gameKey);
    }

    [Fact]
    public async Task Test_Debug_GetSet_SectorContents()
    {
        string testContents = "TESTTEST";
        var targetCoord = new CoordDTO(0, 1);
        var data = await GameInfo.GetGameKey();


        await DebugMethods.SetSectorContents(data, targetCoord, testContents);
        var result = await DebugMethods.GetSectorContents(data, targetCoord);



        Assert.NotNull(result);
        Assert.Equal(testContents, result);
    }

    [Fact]
    public async Task Test_Debug_GetSet_Enemy()
    {
        int shieldLevel = 1234;
        var targetCoord = new CoordDTO(0, 1);
        var data = await GameInfo.GetGameKey();


        await DebugMethods.SetEnemy(data, targetCoord, shieldLevel);
        var result = await DebugMethods.GetEnemy(data, targetCoord);

        Assert.NotNull(result);
        Assert.Equal(shieldLevel, result);
    }

    [Fact]
    public async Task Test_Debug_GetSet_Quadrant()
    {
        CoordDTO targetCoord = new CoordDTO(0, 1);
        QuadrantDTO quadrant = new QuadrantDTO(targetCoord, 123, 321, true, true);
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetQuadrant(data, targetCoord, quadrant);
        var result = await DebugMethods.GetQuadrant(data, targetCoord);


        Assert.NotNull(result);
        Assert.Equal(quadrant.NumberOfEnemies, result.NumberOfEnemies);
        Assert.Equal(quadrant.NumberOfStars, result.NumberOfStars);
        Assert.Equal(quadrant.HasBeenExplored, result.HasBeenExplored);
        Assert.Equal(quadrant.HasStarBase, result.HasStarBase);
    }

    [Fact]
    public async Task Test_Debug_GetSet_Starship()
    {
        CoordDTO targetCoord = new CoordDTO(0, 1);
        var starship = new StarshipDTO();
        starship.EnergyLevel = 1234;
        starship.ShieldLevel = 4321;
        starship.isDestroyed = true;
        starship.isDocked = true;
        starship.Devices.First(z => z.Name == "COM").DamageLevel = -4;
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarship(data, starship);
        var result = await DebugMethods.GetStarship(data);


        Assert.NotNull(result);
        Assert.Equal(starship.ShieldLevel, result.ShieldLevel);
        Assert.Equal(starship.EnergyLevel, result.EnergyLevel);
        Assert.Equal(starship.isDestroyed, result.isDestroyed);
        Assert.Equal(starship.isDocked, result.isDocked);
        Assert.Equal(starship.Devices.First(z => z.Name == "COM").DamageLevel, result.Devices.First(z => z.Name == "COM").DamageLevel);
    }

    [Fact]
    public async Task Test_Debug_GetSet_Galaxy()
    {
        CoordDTO targetCoord = new CoordDTO(0, 1);
        double missionTime = 12345.3;
        double missionTimeDeadline = 4432.1;
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetGalaxy(data, missionTime, missionTimeDeadline);
        var result = await DebugMethods.GetGalaxy(data);


        Assert.NotNull(result.missionTime);
        Assert.NotNull(result.missionTimeDeadline);
        Assert.Equal(missionTime, result.missionTime);
        Assert.Equal(missionTimeDeadline, result.missionTimeDeadline);
    }

    [Fact]
    public async Task Test_Debug_GetSet_ClearCurrentQuadrant_NotAll()
    {
        CoordDTO targetCoord = new CoordDTO(0, 1);
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, false);
        var result = await DebugMethods.GetSectorsByContent(data, Global.SectorEmpty);

        Assert.NotNull(result);
        Assert.Equal(63, result.Count);
    }

    [Fact]
    public async Task Test_Debug_GetSet_ClearCurrentQuadrant_ALL()
    {
        CoordDTO targetCoord = new CoordDTO(0, 1);
        var data = await GameInfo.GetGameKey();

        await DebugMethods.ClearAllCQ(data, true);
        var result = await DebugMethods.GetSectorsByContent(data, Global.SectorEmpty);

        Assert.NotNull(result);
        Assert.Equal(64, result.Count);
    }

    [Fact]
    public async Task Test_Debug_GetSet_StarshipQuadrant()
    {
        var targetCoord = new CoordDTO(3, 5);
        var data = await GameInfo.GetGameKey();

        await DebugMethods.SetStarshipQuadrant(data, targetCoord);
        var result = await DebugMethods.GetStarshipQuadrant(data);

        Assert.NotNull(result);
        Assert.Equal(targetCoord, result);
    }
}
