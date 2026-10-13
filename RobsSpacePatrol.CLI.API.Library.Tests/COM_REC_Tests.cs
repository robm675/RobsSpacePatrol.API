using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobsSpacePatrol.CLI.API.Library;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class COM_REC_Tests
{
    [Fact]
    public async Task test_COM_REC_Working()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var RES = cl.ExecuteCommandString("COM REC");

        Assert.NotEqual(string.Empty, RES);
    }

    [Fact]
    public async Task test_COM_REC_Damaged()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE COM -3");
        var RES = cl.ExecuteCommandString("COM REC");

        Assert.Equal(CLI_Messages.CPU_Damaged, RES);
    }
}