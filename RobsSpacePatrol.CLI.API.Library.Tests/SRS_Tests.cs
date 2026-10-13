using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class SRS_Tests
{
    [Fact]
    public async Task test_SRS_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var RES = cl.ExecuteCommandString("SRS");

        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_SRS_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET STARSHIP DEVICE SRS -3");
        var RES = cl.ExecuteCommandString("SRS");

        Assert.Equal(CLI_Messages.SRS_Damaged, RES);
    }

    [Fact]
    public async Task test_SRS_VerifyMissionTimeChanged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var STBefore = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");
        var RES = cl.ExecuteCommandString("SRS");
        var STAfter = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");

        Assert.NotEqual(STBefore, STAfter);
    }

    [Fact]
    public async Task test_SRS_Contents()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 1 2 STAR");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 3 4 STAR");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 5 7 STAR");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 1 1 STARBASE");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 7 1 ENEMY");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 7 3 ENEMY");

        var RES = cl.ExecuteCommandString("SRS");

        Assert.Equal(3, Regex.Matches(RES, Regex.Escape("***")).Count);
        Assert.Equal(2, Regex.Matches(RES, Regex.Escape("XXX")).Count);
        Assert.Single(Regex.Matches(RES, Regex.Escape("SSS")));
        Assert.Single(Regex.Matches(RES, Regex.Escape("BBB")));
    }
}