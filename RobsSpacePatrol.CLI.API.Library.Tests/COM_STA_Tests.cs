using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

public class COM_STA_Tests
{
    [Fact]
    public async Task test_COM_STA_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var RES = cl.ExecuteCommandString("COM STA");

        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_COM_STA_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE COM -3");
        var RES = cl.ExecuteCommandString("COM STA");

        Assert.Equal(CLI_Messages.CPU_Damaged, RES);
    }

    [Fact]
    public async Task test_COM_STA_VerifyContents()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var RES = cl.ExecuteCommandString("COM STA");
        cl.ExecuteCommandString($"DEBUG SET SKIPENEMYMOVE ON");

        Assert.Contains(CLI_Messages.CPU_STA_DevicesDamaged, RES);
        Assert.Contains(CLI_Messages.CPU_STA_Docked, RES);
        Assert.Contains(CLI_Messages.CPU_STA_EnemiesRemaining, RES);
        Assert.Contains(CLI_Messages.CPU_STA_EnergyRemaining, RES);
        Assert.Contains(CLI_Messages.CPU_STA_Quadrant, RES);
        Assert.Contains(CLI_Messages.CPU_STA_Sector, RES);
        Assert.Contains(CLI_Messages.CPU_STA_ShieldLevel, RES);
        Assert.Contains(CLI_Messages.CPU_STA_StarbasesRemaining, RES);
        Assert.Contains(CLI_Messages.CPU_STA_MissionTime, RES);
        Assert.Contains(CLI_Messages.CPU_STA_MissionTimeUnitsRemaining, RES);
        Assert.Contains(CLI_Messages.CPU_STA_TorpedoRemaining, RES);
    }
}
