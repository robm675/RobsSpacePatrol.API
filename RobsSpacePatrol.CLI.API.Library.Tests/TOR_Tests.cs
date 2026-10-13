using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class TOR_Tests
{
    [Fact]
    public async Task test_TOR_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");


        var RES = cl.ExecuteCommandString("TOR 1");

        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_TOR_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");


        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE TOR -3");
        var RES = cl.ExecuteCommandString("TOR 1");

        Assert.Equal(CLI_Messages.TOR_Damaged, RES);
    }

    [Fact]
    public async Task test_TOR_VerifyMissionTimeChanged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        var STBefore = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");
        var RES = cl.ExecuteCommandString("TOR 7");
        var STAfter = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");

        Assert.NotEqual(STBefore, STAfter);
    }

    [Fact]
    public async Task test_TOR_NoEnemies()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");

        var RES = cl.ExecuteCommandString("TOR 1");

        Assert.Equal(CLI_Messages.TOR_NoEnemies, RES);
    }

    [Fact]
    public async Task test_TOR_InvalidDirection()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");

        var RES = cl.ExecuteCommandString("TOR 9");

        Assert.StartsWith(CLI_Messages.TOR_InvalidDirection, RES);
    }

    [Fact]
    public async Task test_TOR_VerifyHitResults_EnemyDestroyed()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        var RES = cl.ExecuteCommandString("TOR 7");


        Assert.Contains(CLI_Messages.TOR_Fired, RES);
        Assert.Contains(CLI_Messages.TOR_EnemyAtSector, RES);
        Assert.Contains(CLI_Messages.TOR_Destroyed, RES);
    }

    [Fact]
    public async Task test_TOR_VerifyHitResults_StarBaseDestroyed()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 7 STARBASE");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 4 5 100");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");


        var RES = cl.ExecuteCommandString("TOR 7");


        Assert.Contains(CLI_Messages.TOR_Fired, RES);
        Assert.Contains(CLI_Messages.TOR_StarBaseDestroyed, RES);
    }

    [Fact]
    public async Task test_TOR_InsufficientTorps()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP TORP 0");
        var RES = cl.ExecuteCommandString("TOR 1");

        Assert.Equal(CLI_Messages.TOR_Expended, RES);
    }

    [Fact]
    public async Task test_TOR_Missed()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 5 7 100");

        var RES = cl.ExecuteCommandString("TOR 6");

        Assert.Contains(CLI_Messages.TOR_Missed, RES);
    }

    [Fact]
    public async Task test_TOR_VerifyHitResults_EnemyDestroyedCoords()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var resp1 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 7 100");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 6 100");
        var resp3 = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 0 5 100");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        var RES = cl.ExecuteCommandString("TOR 7");

        Assert.Contains(CLI_Messages.TOR_Fired, RES);
        Assert.Contains(CLI_Messages.TOR_EnemyAtSector, RES);
        Assert.Contains(CLI_Messages.TOR_Destroyed, RES);
        Assert.Contains("0.5", RES);
    }
}