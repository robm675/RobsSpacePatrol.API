using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class NAV_Tests
{
    [Fact]
    public async Task test_NAV_InvalidNumberOfParms()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();


        var RES = cl.ExecuteCommandString("NAV");

        Assert.Contains(CLI_Messages.NAV_InvalidCommand, RES);
    }

    [Fact]
    public async Task test_NAV_InvalidDIRParm_Range()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();


        var RES = cl.ExecuteCommandString("NAV 9 1");


        Assert.Contains(CLI_Messages.NAV_Invalid_DIR + "9", RES);
    }

    [Fact]
    public async Task test_NAV_InvalidDIRParm_Alpha()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();


        var RES = cl.ExecuteCommandString("NAV a 1");

        Assert.Equal(CLI_Messages.NAV_InvalidDIRValue + "a", RES);
    }

    [Fact]
    public async Task test_NAV_InvalidDISTParm_Range()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();


        var RES = cl.ExecuteCommandString("NAV 1 9");

        Assert.Contains(CLI_Messages.NAV_Invalid_DIST + "9", RES);
    }

    [Fact]
    public async Task test_NAV_InvalidDISTParm_Alpha()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();


        var RES = cl.ExecuteCommandString("NAV 1 a");

        Assert.Equal(CLI_Messages.NAV_InvalidDISTValue + "a", RES);
    }

    [Fact]
    public async Task test_NAV_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        cl.ExecuteCommandString("DEBUG SET CLEARCQ");

        var RES = cl.ExecuteCommandString("NAV 1 2");
        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_NAV_Working_VerifyLocation_EW()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);



        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        var RES = cl.ExecuteCommandString("NAV 1 2");
        var resp2 = cl.ExecuteCommandString("DEBUG GET STARSHIP QUADRANT BRIEF");

        Assert.Equal("2.0", resp2);
    }

    [Fact]
    public async Task test_NAV_Working_VerifyLocation_NS()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);



        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");


        var RES = cl.ExecuteCommandString("NAV 7 2");
        var resp2 = cl.ExecuteCommandString("DEBUG GET STARSHIP QUADRANT BRIEF");

        Assert.Equal("0.2", resp2);
    }

    [Fact]
    public async Task test_NAV_Damaged_DistTooLarge()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE NAV -3");
        var RES = cl.ExecuteCommandString("NAV 1 2");

        Assert.Equal(CLI_Messages.NAV_SpaceDriveDamaged, RES);
    }

    [Fact]
    public async Task test_NAV_Damaged_DistOK()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ");

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE NAV -3");
        var RES = cl.ExecuteCommandString("NAV 1 .2");
        Assert.Contains(CLI_Messages.NAV_ImpulseEngaged, RES);
    }

    [Fact]
    public async Task test_NAV_VerifyMissionTimeChanged_SpaceDrive()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");

        var STBefore = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");
        var RES = cl.ExecuteCommandString("NAV 1 2");
        var STAfter = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");

        Assert.NotEqual(STBefore, STAfter);
    }

    [Fact]
    public async Task test_NAV_VerifyMissionTimeChanged_ImpulseDrive()
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
        var RES = cl.ExecuteCommandString("NAV 1 .2");
        var STAfter = cl.ExecuteCommandString("DEBUG GET GALAXY MISSIONTIME BRIEF");

        Assert.NotEqual(STBefore, STAfter);
    }

    [Fact]
    public async Task test_NAV_OutsideGalaxy()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");


        var RES = cl.ExecuteCommandString("NAV 5 1");

        Assert.Equal(CLI_Messages.NAV_OutsideGalaxy, RES);
    }

    [Fact]
    public async Task test_NAV_SpaceDriveEngaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ");


        var RES = cl.ExecuteCommandString("NAV 1 2");

        Assert.Contains(CLI_Messages.NAV_SpaceDriveEngaged, RES);
    }

    [Fact]
    public async Task test_NAV_ImpulseDrivesShutDown()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 7 0 STAR");

        var RES = cl.ExecuteCommandString("NAV 1 2");

        Assert.Contains(CLI_Messages.NAV_ImpulseEnginesShutDown, RES);
        Assert.Contains(CLI_Messages.NAV_BadNavigation, RES);
    }

    [Fact]
    public async Task test_NAV_NotEnoughEnergyForTrip_ShieldEnergyAvailable()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");

        var resp1 = cl.ExecuteCommandString($"DEBUG SET STARSHIP ENERGY 0");
        var resp2 = cl.ExecuteCommandString($"DEBUG SET STARSHIP SHIELD 1000");

        var RES = cl.ExecuteCommandString("NAV 1 1");

        Assert.Contains(CLI_Messages.NAV_NotEnoughEnergy_ShieldEnergyAvail, RES);
    }

    [Fact]
    public async Task test_Nav_ArriveInNewQuadrant_EnemyPresent_NoShieldsGetWarningMessage()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString($"DEBUG SET STOPENEMYFIRE");
        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        cl.ExecuteCommandString($"DEBUG SET QUADRANT 1 0 NUMENEMIES 3");


        var RES = cl.ExecuteCommandString("NAV 1 1");

        Assert.Contains(CLI_Messages.WarningMessageShieldDownInCombatArea, RES);
    }

    [Fact]
    public async Task test_Nav_ArriveInNewQuadrant_EnemyPresent_GetRedAlertMessage()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);


        cl.ExecuteCommandString($"DEBUG SET STOPENEMYFIRE");
        cl.ExecuteCommandString($"DEBUG SET STARSHIP QUADRANT 0 0");
        cl.ExecuteCommandString("DEBUG SET CLEARCQ ALL");
        cl.ExecuteCommandString($"DEBUG SET CURRENTQUADRANT SECTOR 0 0 STARSHIP");
        cl.ExecuteCommandString($"DEBUG SET QUADRANT 1 0 NUMENEMIES 3");
        cl.ExecuteCommandString($"DEBUG SET STARSHIP SHIELD 1000");


        var RES = cl.ExecuteCommandString("NAV 1 1");

        Assert.Contains(CLI_Messages.MAINT_NewQuadrantEnemiesRedAlert, RES);
    }
}