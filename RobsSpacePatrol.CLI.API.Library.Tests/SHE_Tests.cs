using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class SHE_Tests
{
    [Fact]
    public async Task test_SHE_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        var RES = cl.ExecuteCommandString("SHE 1000");

        Assert.StartsWith(CLI_Messages.SHE_Changed, RES);
        Assert.Contains("1000", RES);
    }

    [Fact]
    public async Task test_SHE_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString("DEBUG SET STARSHIP DEVICE SHE -3");
        var RES = cl.ExecuteCommandString("SHE 1000");

        Assert.Equal(CLI_Messages.SHE_Damaged, RES);
    }

    [Fact]
    public async Task test_SHE_MissingAmount()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();


        var RES = cl.ExecuteCommandString("SHE");

        Assert.Equal(CLI_Messages.SHE_MissingAmount, RES);
    }

    [Fact]
    public async Task test_SHE_InvalidAmount()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();


        var RES = cl.ExecuteCommandString("SHE 4000");

        Assert.StartsWith(CLI_Messages.SHE_InvalidAmount, RES);
    }

    [Fact]
    public async Task test_SHE_VerifyMissionTimeChanged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        var STBefore = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");
        var RES = cl.ExecuteCommandString("SHE 1000");
        var STAfter = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        Assert.NotEqual(STBefore, STAfter);
    }
}