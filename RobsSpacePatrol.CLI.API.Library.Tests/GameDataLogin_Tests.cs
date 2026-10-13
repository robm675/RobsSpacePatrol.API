using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class GameDataLogin_Tests
{
    [Fact]
    public async Task StartGame_InvalidCredentials_ReportLoginFailure()
    {
        const string incorrectPassword = "DefinitelyNotTheCorrectPassword123$";
        var unknownEmail = $"missing-{Guid.NewGuid():N}@example.test";

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => GameData.StartGame(
                ApiTestSettings.BaseUrl,
                unknownEmail,
                incorrectPassword));

        Assert.Equal(
            "Login failed. Check the email address and password.",
            exception.Message);
        Assert.DoesNotContain(incorrectPassword, exception.Message);
    }
}