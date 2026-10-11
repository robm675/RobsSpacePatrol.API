using RobsSpacePatrol.CLI.API.Library;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

public class GameBuilder : IAsyncDisposable
{
    private GameData? gameData;

    public async Task<ICommandLine> getCommandLine(bool useAdmin = false)
    {
        gameData = await getGameData(useAdmin);
        var debug = new DebugCommands_API(gameData);
        return new CommandLine_API(gameData, debug);
    }

    public async Task<GameData> getGameData(bool useAdmin = false)
    {
        var email = useAdmin ? ApiTestSettings.AdminEmail : ApiTestSettings.UserEmail;
        return await GameData.StartGame(ApiTestSettings.BaseUrl, email, ApiTestSettings.Password);
    }

    public async ValueTask DisposeAsync()
    {
        if (gameData == null)
            return;

        try
        {
            await GameData.DeleteGame(gameData);
        }
        finally
        {
            gameData.Dispose();
        }
    }
}
