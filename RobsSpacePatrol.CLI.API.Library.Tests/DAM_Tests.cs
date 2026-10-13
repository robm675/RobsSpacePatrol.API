using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class DAM_Tests
{
    [Fact]
    public async Task test_DAM_basics()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine();

        var RES = cl.ExecuteCommandString("DAM");

        Assert.Contains(CLI_Messages.DAM_DamageReport, RES);
    }

    [Fact]
    public async Task test_DAM_showed_damages()
    {
        await using var gb = new GameBuilder();
        var cl = await gb.getCommandLine(true);

        var resp = cl.ExecuteCommandString($"DEBUG SET STARSHIP DEVICE NAV -3");
        var RES = cl.ExecuteCommandString("DAM");

        Assert.Contains("DAMAGED", RES.ToUpper());
    }

}