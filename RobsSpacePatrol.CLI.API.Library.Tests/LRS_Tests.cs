using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class LRS_Tests
{
    [Fact]
    public async Task test_LRS_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var result = cl.ExecuteCommandString("LRS");

        Assert.NotEqual(string.Empty, result);
    }

    [Fact]
    public async Task test_LRS_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET STARSHIP DEVICE LRS -3");

        var result = cl.ExecuteCommandString("LRS");

        Assert.Equal(CLI_Messages.LRS_Damaged, result);
    }

    [Fact]
    public async Task test_LRS_VerifyMissionTimeChanged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var missionTimeBefore = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");
        cl.ExecuteCommandString("LRS");
        var missionTimeAfter = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");

        Assert.NotEqual(missionTimeBefore, missionTimeAfter);
    }

    [Fact]
    public async Task test_LRS_Contents()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 0 0 NUMSTARS 5");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 0 0 STARBASE 0");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 0 0 NUMENEMIES 3");

        cl.ExecuteCommandString("DEBUG SET QUADRANT 1 0 NUMSTARS 1");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 1 0 STARBASE 1");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 1 0 NUMENEMIES 0");

        cl.ExecuteCommandString("DEBUG SET QUADRANT 0 1 NUMSTARS 1");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 0 1 STARBASE 1");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 0 1 NUMENEMIES 0");

        cl.ExecuteCommandString("DEBUG SET QUADRANT 1 1 NUMSTARS 9");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 1 1 STARBASE 0");
        cl.ExecuteCommandString("DEBUG SET QUADRANT 1 1 NUMENEMIES 4");

        var result = cl.ExecuteCommandString("LRS");
        var lines = result.Split('\n');

        Assert.NotEqual(string.Empty, result);
        Assert.Equal("| | *** | *** | *** |", lines[2]);
        Assert.Equal("Y | *** | 305 | 011 |", lines[4]);
        Assert.Equal("| | *** | 011 | 409 |", lines[6]);
    }
}