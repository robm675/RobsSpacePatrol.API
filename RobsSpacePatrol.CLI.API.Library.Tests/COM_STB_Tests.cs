using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

public class COM_STB_Tests
{
    [Fact]
    public async Task test_COM_STB_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var RES = cl.ExecuteCommandString("COM STB");

        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_COM_STB_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE COM -3");
        var RES = cl.ExecuteCommandString("COM STB");

        Assert.Equal(CLI_Messages.CPU_Damaged, RES);
    }

    [Fact]
    public async Task test_COM_STB_VerifyContents()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 1 STARBASE");

        var RES = cl.ExecuteCommandString("COM STB");

        Assert.Contains(CLI_Messages.CPU_STB_Dir, RES);
        Assert.Contains(CLI_Messages.CPU_STB_Dist, RES);
        Assert.Contains(CLI_Messages.CPU_STB_StarbaseFoundInSector, RES);
    }

    [Fact]
    public async Task test_COM_STB_NoStarbases()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        var RES = cl.ExecuteCommandString("COM STB");

        Assert.Contains(CLI_Messages.CPU_STB_NoStarBases, RES);
    }
}