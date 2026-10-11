using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

public class COM_NAV_Tests
{
    [Fact]
    public async Task test_COM_NAV_InvalidQX_Range()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV 9 2 3 4");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidCoord, RES);
    }

    [Fact]
    public async Task test_COM_NAV_InvalidQX_Alpha()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV a 2 3 4");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidQX, RES);
    }

    [Fact]
    public async Task test_COM_NAV_InvalidQY_Range()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV 1 9 3 4");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidCoord, RES);
    }

    [Fact]
    public async Task test_COM_NAV_InvalidQY_Alpha()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV 1 a 3 4");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidQY, RES);
    }

    [Fact]
    public async Task test_COM_NAV_InvalidSX_Range()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV 1 2 9 4");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidCoord, RES);
    }

    [Fact]
    public async Task test_COM_NAV_InvalidSX_Alpha()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV 1 2 a 4");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidSX, RES);
    }

    [Fact]
    public async Task test_COM_NAV_InvalidSY_Range()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV 1 2 3 9");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidCoord, RES);
    }

    [Fact]
    public async Task test_COM_NAV_InvalidSY_Alpha()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM NAV 1 2 3 a");

        Assert.Contains(CLI_Messages.CPU_NAV_InvalidSY, RES);
    }

    [Fact]
    public async Task test_COM_NAV_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ");

        var RES = cl.ExecuteCommandString("COM NAV 1 2 3 4");

        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_COM_NAV_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE COM -3");
        var RES = cl.ExecuteCommandString("COM NAV 1 2 3 4");

        Assert.Equal(CLI_Messages.CPU_Damaged, RES);
    }

    [Fact]
    public async Task test_COM_NAV_VerifyContents()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var RES = cl.ExecuteCommandString("COM NAV 0 7 0 0");

        Assert.Contains(CLI_Messages.CPU_NAV_Dir, RES);
        Assert.Contains(CLI_Messages.CPU_NAV_Dist, RES);
    }
}