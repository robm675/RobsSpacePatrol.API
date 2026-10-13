using System.Net;
using System.Net.Http.Json;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class GameDataApiResponse_Tests
{
    [Fact]
    public async Task PostForApiResponseAsync_Success_ReturnsTypedResponse()
    {
        using var handler = new FixedResponseHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    data = "expected value"
                })
            });
        using var client = new HttpClient(handler);
        using var gameData = CreateGameData(client);

        var response = await gameData.PostForApiResponseAsync<object, string>(
            "api/game/example",
            new { });

        Assert.Equal("expected value", response.Data);
        Assert.Equal("/api/game/example", handler.RequestPath);
    }

    [Fact]
    public async Task PostForApiResponseAsync_FailedHttpResponse_ReportsHttpErrorBeforeReadingJson()
    {
        using var handler = new FixedResponseHandler(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("This is deliberately not JSON.")
            });
        using var client = new HttpClient(handler);
        using var gameData = CreateGameData(client);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => gameData.PostForApiResponseAsync<object, string>(
                "api/game/example",
                new { }));

        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public async Task PostForApiResponseAsync_NullJsonResponse_ReportsEndpoint()
    {
        using var handler = new FixedResponseHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create<object?>(null)
            });
        using var client = new HttpClient(handler);
        using var gameData = CreateGameData(client);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => gameData.PostForApiResponseAsync<object, string>(
                "api/game/example",
                new { }));

        Assert.Contains("api/game/example", exception.Message);
    }

    private static GameData CreateGameData(HttpClient client)
    {
        return new GameData(
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