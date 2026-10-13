using System.Net;
using System.Net.Http.Json;
using RobsSpacePatrol.CLI.API.Library;
using Xunit;

namespace RobsSpacePatrol.CLI.API.Library.Tests;

[Trait("Category", "Database")]
public class GameDataRefresh_Tests
{
    [Fact]
    public async Task PostAsJsonAsync_ExpiringToken_RefreshesAndUsesReplacementToken()
    {
        using var handler = new RecordingHandler();
        using var client = new HttpClient(handler);
        var gameData = new GameData(
            "https://example.test",
            "player@example.test",
            CreateToken("old-access", "old-refresh", expiresIn: 0),
            client,
            Guid.NewGuid());

        using var response = await gameData.PostAsJsonAsync(
            "api/game/nav",
            new { Direction = 1.0, Distance = 0.2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Collection(
            handler.Requests,
            refreshRequest =>
            {
                Assert.Equal("/api/login/refresh", refreshRequest.Path);
                Assert.Contains("old-refresh", refreshRequest.Body);
            },
            gameRequest =>
            {
                Assert.Equal("/api/game/nav", gameRequest.Path);
                Assert.Equal("Bearer", gameRequest.AuthenticationScheme);
                Assert.Equal("new-access", gameRequest.AuthenticationParameter);
            });
        Assert.Equal("new-access", gameData.TokenData.AccessToken);
        Assert.Equal("new-refresh", gameData.TokenData.RefreshToken);
    }

    [Fact]
    public async Task PostAsJsonAsync_ValidToken_DoesNotRefresh()
    {
        using var handler = new RecordingHandler();
        using var client = new HttpClient(handler);
        var gameData = new GameData(
            "https://example.test",
            "player@example.test",
            CreateToken("current-access", "current-refresh", expiresIn: 3600),
            client,
            Guid.NewGuid());

        using var response = await gameData.PostAsJsonAsync(
            "/api/game/srs",
            new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("/api/game/srs", request.Path);
        Assert.Equal("Bearer", request.AuthenticationScheme);
        Assert.Equal("current-access", request.AuthenticationParameter);
    }

    private static IdentityTokenResponse CreateToken(
        string accessToken,
        string refreshToken,
        long expiresIn)
    {
        return new IdentityTokenResponse
        {
            TokenType = "Bearer",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresIn
        };
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content == null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);

            Requests.Add(new RecordedRequest(
                request.RequestUri!.AbsolutePath,
                body,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter));

            if (request.RequestUri.AbsolutePath == "/api/login/refresh")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        CreateToken("new-access", "new-refresh", expiresIn: 3600))
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed record RecordedRequest(
        string Path,
        string Body,
        string? AuthenticationScheme,
        string? AuthenticationParameter);
}