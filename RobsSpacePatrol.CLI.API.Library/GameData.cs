using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Game;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RobsSpacePatrol.CLI.API.Library;

public class GameData : IDisposable
{
    internal GameData(string baseURL, string eMail, IdentityTokenResponse tokenData, HttpClient client, Guid gameID)
    {
        BaseURL = baseURL;
        this.eMail = eMail;
        TokenData = tokenData;
        Client = client;
        GameID = gameID;
        SetToken(tokenData);
    }

    private bool disposed;
    public string BaseURL { get; }
    public string eMail { get; }
    public IdentityTokenResponse TokenData { get; private set; }
    public HttpClient Client { get; }
    public Guid GameID { get; }
    private DateTimeOffset accessTokenExpiresAtUtc;
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    private void SetToken(IdentityTokenResponse token)
    {
        TokenData = token;

        accessTokenExpiresAtUtc = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
    }

    private async Task EnsureValidAccessTokenAsync()
    {
        // Refresh one minute before expiration.
        if (DateTimeOffset.UtcNow < accessTokenExpiresAtUtc.Subtract(TimeSpan.FromMinutes(1)))
            return;

        await refreshLock.WaitAsync();

        try
        {
            // Another request may already have refreshed it.
            if (DateTimeOffset.UtcNow < accessTokenExpiresAtUtc.Subtract(TimeSpan.FromMinutes(1)))
                return;

            using var response = await Client.PostAsJsonAsync($"{BaseURL}/api/login/refresh",
                new
                {
                    refreshToken = TokenData.RefreshToken
                });

            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<IdentityTokenResponse>();

            if (token == null || string.IsNullOrWhiteSpace(token.AccessToken) || string.IsNullOrWhiteSpace(token.RefreshToken))
                throw new InvalidOperationException("Token refresh did not return valid tokens.");

            SetToken(token);
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string endpoint, T request)
    {
        await EnsureValidAccessTokenAsync();

        return await Client.PostAsJsonAsync($"{BaseURL}/{endpoint.TrimStart('/')}", request);
    }

    public async Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        await EnsureValidAccessTokenAsync();

        return await Client.GetAsync($"{BaseURL}/{endpoint.TrimStart('/')}");
    }

    private static async Task Register(HttpClient client, string baseURL, string email, string password)
    {
        var request = new RegistrationRequestDTO
        {
            Email = email,
            Password = password
        };

        using var response = await client.PostAsJsonAsync($"{baseURL}/api/login/register", request);

        response.EnsureSuccessStatusCode();
    }

    private static async Task<IdentityTokenResponse> DoLogin(HttpClient client, string baseURL, string eMail, string password)
    {
        var loginRequestDTO = new LoginRequestDTO()
        {
            Email = eMail,
            Password = password,
        };


        var requestUrl = $"{baseURL}/api/login/login?useCookies=false";

        using var loginResponse = await client.PostAsJsonAsync(requestUrl, loginRequestDTO);

        if (!loginResponse.IsSuccessStatusCode)
        {
            if (loginResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException("Login failed. Check the email address and password.");
            }
            loginResponse.EnsureSuccessStatusCode();
        }

        var tokenResponse = await loginResponse.Content.ReadFromJsonAsync<IdentityTokenResponse>();

        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new Exception("Login did not return an access token.");
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);

        return tokenResponse;
    }

    internal static async Task<Guid> StartGame(HttpClient client, string baseUrl)
    {
        using var response = await client.GetAsync($"{baseUrl}/api/game/start_game");

        response.EnsureSuccessStatusCode();

        var gameStartResponse = await response.Content.ReadFromJsonAsync<ApiResponse<GameStartResponse>>();

        if (gameStartResponse?.Data == null)
        {
            throw new InvalidOperationException("Starting the game did not return a game ID.");
        }

        return gameStartResponse.Data.GameID;
    }

    public static async Task<GameData> RegisterAndStartNewGame(string baseURL, string eMail, string password)
    {
        HttpClient Client = new();
        try
        {
            await Register(Client, baseURL, eMail, password);
            var token = await DoLogin(Client, baseURL, eMail, password);
            var gameId = await StartGame(Client, baseURL);

            return new GameData(baseURL, eMail, token, Client, gameId);
        }
        catch (Exception)
        {
            Client.Dispose();
            throw;
        }
    }

    public static async Task<GameData> StartGame(string baseURL, string eMail, string password)
    {
        HttpClient Client = new();
        try
        {
            var token = await DoLogin(Client, baseURL, eMail, password);
            var gameId = await StartGame(Client, baseURL);

            return new GameData(baseURL, eMail, token, Client, gameId);
        }
        catch (Exception)
        {
            Client.Dispose();
            throw;
        }
    }

    public static async Task<bool> DeleteGame(GameData gameData)
    {
        var req = new ZAP_Game_Request(gameData.GameID);
        using var response = await gameData.PostAsJsonAsync("api/game/zap_game", req);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<DeleteGameResponse>>();

        if (data?.Data == null)
            throw new InvalidOperationException("Deleting the game did not return a result.");

        return data.Data.deletedOK;
    }

    public async Task<ApiResponse<TResponse>> PostForApiResponseAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        using var response = await PostAsJsonAsync(endpoint, request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>();

        if (result == null)
        {
            throw new InvalidOperationException($"The API returned no usable response for '{endpoint}'.");
        }

        return result;
    }

    public void Dispose()
    {
        if (disposed)
            return;

        Client.Dispose();
        refreshLock.Dispose();
        disposed = true;
    }

}
