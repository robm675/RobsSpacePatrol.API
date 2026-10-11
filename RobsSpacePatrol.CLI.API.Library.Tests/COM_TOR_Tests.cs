using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

public class COM_TOR_Tests
{
    [Fact]
    public async Task test_COM_TOR_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");


        var RES = cl.ExecuteCommandString("COM TOR");

        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_COM_TOR_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE COM -3");
        var RES = cl.ExecuteCommandString("COM TOR");

        Assert.Equal(CLI_Messages.CPU_Damaged, RES);
    }

    [Fact]
    public async Task test_COM_TOR_CheckDirections()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");
        var RES = cl.ExecuteCommandString("COM TOR");

        Assert.Equal(3, new Regex(Regex.Escape(CLI_Messages.CPU_TOR_EnemyAtSector)).Matches(RES).Count);
        Assert.Equal(3, new Regex(Regex.Escape(CLI_Messages.CPU_TOR_IsBearing)).Matches(RES).Count);

        Assert.Single(new Regex(Regex.Escape("[0.7]") + " is bearing").Matches(RES));
        Assert.Single(new Regex(Regex.Escape("[0.6]") + " is bearing").Matches(RES));
        Assert.Single(new Regex(Regex.Escape("[0.5]") + " is bearing").Matches(RES));
    }

    [Fact]
    public async Task test_COM_TOR_NoEnemies()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("COM TOR");

        Assert.Equal(CLI_Messages.CPU_TOR_NoEnemies, RES);
    }
}
