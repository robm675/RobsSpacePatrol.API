using System.Net;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

public class GameDataStart_Tests
{
    [Fact]
    public async Task StartGame_FailedHttpResponse_ReportsHttpErrorBeforeReadingJson()
    {
        using var handler = new FixedResponseHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("This is deliberately not JSON.")
            });
        using var client = new HttpClient(handler);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => GameData.StartGame(client, "https://example.test"));

        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    private sealed class FixedResponseHandler(HttpResponseMessage response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }
}
