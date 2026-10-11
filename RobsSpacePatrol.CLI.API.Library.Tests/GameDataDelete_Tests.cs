using System.Net;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

public class GameDataDelete_Tests
{
    [Fact]
    public async Task DeleteGame_FailedHttpResponse_ReportsHttpErrorBeforeReadingJson()
    {
        using var handler = new FixedResponseHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("This is deliberately not JSON.")
            });
        using var client = new HttpClient(handler);
        var gameData = new GameData(
            "https://example.test",
            "player@example.test",
            new IdentityTokenResponse
            {
                TokenType = "Bearer",
                AccessToken = "access-token",
                RefreshToken = "refresh-token",
                ExpiresIn = 3600
            },
            client,
            Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => GameData.DeleteGame(gameData));

        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
        Assert.Equal("/api/game/zap_game", handler.RequestPath);
    }

    private sealed class FixedResponseHandler(HttpResponseMessage response)
        : HttpMessageHandler
    {
        public string? RequestPath { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestPath = request.RequestUri?.AbsolutePath;
            return Task.FromResult(response);
        }
    }
}
