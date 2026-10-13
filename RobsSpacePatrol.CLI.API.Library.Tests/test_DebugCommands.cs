using Xunit;
using RobsSpacePatrol.DTO.Library;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class test_DebugCommands
{
    [Fact]
    public async Task test_DebugOK_CHECK()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var resp = cl.ExecuteCommandString("DEBUG CHECK");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
    }

    [Fact]
    public async Task test_DebugSetGetSectorContents()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var contents = "STARSHIP";

        var resp = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 {contents}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET CURRENTQUADRANT SECTOR 0 0");

        Assert.Equal(contents, resp2);
    }

    [Fact]
    public async Task test_DebugSetGetSectorEnemy()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string shieldLevel = "1000";

        var resp = cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT ENEMY 1 1 {shieldLevel}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET CURRENTQUADRANT ENEMY 1 1");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(shieldLevel, resp2);
    }

    [Fact]
    public async Task test_DebugSetGetQuadrantNumEnemies()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string numEnemies = "3";

        var resp = cl.ExecuteCommandString($"DEBUG SET QUADRANT 0 0 NUMENEMIES {numEnemies}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET QUADRANT 0 0 NUMENEMIES");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(numEnemies, resp2);
    }

    [Fact]
    public async Task test_DebugSetGetQuadrantNumStars()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string numStars = "5";

        var resp = cl.ExecuteCommandString($"DEBUG SET QUADRANT 0 0 NUMSTARS {numStars}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET QUADRANT 0 0 NUMSTARS");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(numStars, resp2);
    }

    [Fact]
    public async Task test_DebugSetGetQuadrantStarBase()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string hasSB = "1";

        var resp = cl.ExecuteCommandString($"DEBUG SET QUADRANT 0 0 STARBASE {hasSB}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET QUADRANT 0 0 STARBASE");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(hasSB, resp2);
    }

    [Fact]
    public async Task test_DebugSetGetQuadrantExplored()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string exploredSW = "1";

        var resp = cl.ExecuteCommandString($"DEBUG SET QUADRANT 0 0 EXPLORED {exploredSW}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET QUADRANT 0 0 EXPLORED");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(exploredSW, resp2);
    }

    [Fact]
    public async Task test_DebugSetGet_Starship_Energy()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string energyAmt = "2000";

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP ENERGY {energyAmt}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET STARSHIP ENERGY");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(energyAmt, resp2);
    }

    [Fact]
    public async Task test_DebugSetGet_Starship_ShieldLevel()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string shieldAmt = "1000";

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP SHIELD {shieldAmt}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET STARSHIP SHIELD");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(shieldAmt, resp2);
    }

    [Fact]
    public async Task test_DebugSetGet_Starship_Torps()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string torpAmount = "5";

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP TORP {torpAmount}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET STARSHIP TORP");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(torpAmount, resp2);
    }

    [Fact]
    public async Task test_DebugSetGet_Starship_DeviceDamage()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string damageAmount = "-3";
        string device = "TOR";

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE {device} {damageAmount}");
        var resp2 = cl.ExecuteCommandString($"DEBUG GET STARSHIP DEVICE {device}");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(damageAmount, resp2);
    }

    [Fact]
    public async Task test_DebugSetGet_Galaxy_MissionTime()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string sd = "2400";

        var resp = cl.ExecuteCommandString($"DEBUG SET GALAXY MISSIONTIME {sd}");
        var resp2 = cl.ExecuteCommandString($"DEBUG GET GALAXY MISSIONTIME");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(sd, resp2);
    }

    [Fact]
    public async Task test_DebugSetGet_Galaxy_MissionTimeDeadLine()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string sd = "2410";

        var resp = cl.ExecuteCommandString($"DEBUG SET GALAXY MISSIONTIMEDEADLINE {sd}");
        var resp2 = cl.ExecuteCommandString($"DEBUG GET GALAXY MISSIONTIMEDEADLINE");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains(sd, resp2);
    }

    [Fact]
    public async Task test_DebugSetClearSector()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        int x = 3;
        int y = 4;
        string contents = "STAR";

        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR {x} {y} {contents}");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        var res = cl.ExecuteCommandString($"DEBUG GET CURRENTQUADRANT SECTOR {x} {y}");
        var res2 = cl.ExecuteCommandString($"DEBUG GET CURRENTQUADRANT SECTOR 0 0 BRIEF");

        Assert.Equal(contents, res);
        Assert.Equal("STARSHIP", res2);
    }

    [Fact]
    public async Task test_Debug_GetSet_StarshipQuadrant()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);
        string x = "1";
        string y = "1";

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT {x} {y}");
        var resp2 = cl.ExecuteCommandString("DEBUG GET STARSHIP QUADRANT");

        Assert.Equal(CLI_Messages.DebugCommandOK, resp);
        Assert.Contains($"{x}.{y}", resp2);
    }
}