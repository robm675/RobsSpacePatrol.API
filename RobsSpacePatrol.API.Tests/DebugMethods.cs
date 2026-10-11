using System.Net.Http.Json;
using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Debug.Get;
using RobsSpacePatrol.DTO.Debug.Set;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using Xunit;

namespace RobsSpacePatrol.API.Tests;

public record GameData(HttpClient client, Guid gameKey);

public static class GameInfo
{
    public static async Task<GameData> GetGameKey()
    {
        var factory = new TestFactory(Builder.GameService);
        var client = factory.CreateClient();

        using var startGameResponse = await client.GetAsync("api/game/start_game", TestContext.Current.CancellationToken);
        startGameResponse.EnsureSuccessStatusCode();

        var result = await startGameResponse.Content.ReadFromJsonAsync<ApiResponse<GameStartResponse>>(cancellationToken: TestContext.Current.CancellationToken);
        if (result == null || result.Data == null)
            throw new Exception("Result is null in GetGameKey");

        return new(client, result.Data.GameID);
    }
}

public static class DebugMethods
{
    public static async Task<ApiResponse<TResponse>?> PostApi<TRequest, TResponse>(
            HttpClient client,
            string url,
            TRequest request)
    {
        using var response = await client.PostAsJsonAsync(
            url,
            request,
            TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(
            cancellationToken: TestContext.Current.CancellationToken);
    }

    public static async Task SetSectorContents(GameData gameInfo, CoordDTO coord, string contents)
    {
        var request = new DebugSetSectorContentsRequest(gameInfo.gameKey, coord, contents);
        var response = await PostApi<DebugSetSectorContentsRequest, object>(
            gameInfo.client,
            "api/debug/set/sector_contents",
            request);
    }

    public static async Task<string?> GetSectorContents(GameData gameInfo, CoordDTO coord)
    {
        var request = new DebugGetSectorContentsRequest(gameInfo.gameKey, coord);
        var result = await PostApi<
            DebugGetSectorContentsRequest,
            DebugGetSectorContentsResponse>(
                gameInfo.client,
                "api/debug/get/sector_contents",
                request);
        return result?.Data?.SectorContents;
    }

    public static async Task SetEnemy(GameData gameInfo, CoordDTO coord, int shieldLevel)
    {
        var enemy = new EnemyDTO(shieldLevel);
        var res = await PostApi<DebugSetEnemyRequest, object>(
            gameInfo.client,
            "api/debug/set/enemy",
                new DebugSetEnemyRequest(
                    gameInfo.gameKey,
                    coord,
                    enemy));
    }

    public static async Task<int?> GetEnemy(GameData gameInfo, CoordDTO coord)
    {
        var result = await PostApi<
               DebugGetEnemyRequest,
               DebugGetEnemyResponse>(
                   gameInfo.client,
                   "api/debug/get/enemy",
                   new DebugGetEnemyRequest(
                       gameInfo.gameKey,
                       coord));
        return result?.Data?.enemy?.ShieldLevel;
    }

    public static async Task SetQuadrant(GameData gameInfo, CoordDTO coord, QuadrantDTO quadrant)
    {
        await PostApi<DebugSetQuadrantRequest, object>(
                gameInfo.client,
                "api/debug/set/quadrant",
                new DebugSetQuadrantRequest(
                    gameInfo.gameKey,
                    coord,
                    quadrant));
    }

    public static async Task<QuadrantDTO?> GetQuadrant(GameData gameInfo, CoordDTO coord)
    {
        var result = await PostApi<
             DebugGetQuadrantRequest,
             DebugGetQuadrantResponse>(
                 gameInfo.client,
                 "api/debug/get/quadrant",
                 new DebugGetQuadrantRequest(
                     gameInfo.gameKey,
                     coord));
        return result?.Data?.quadrant;
    }

    public static async Task SetStarship(GameData gameinfo, StarshipDTO starship)
    {
        await PostApi<DebugSetStarshipRequest, object>(
              gameinfo.client,
              "api/debug/set/starship",
              new DebugSetStarshipRequest(
                  gameinfo.gameKey,
                  starship));
    }

    public static async Task<StarshipDTO?> GetStarship(GameData gameInfo)
    {
        var result = await PostApi<
             DebugGetStarshipRequest,
             DebugGetStarshipResponse>(
                 gameInfo.client,
                 "api/debug/get/starship",
                 new DebugGetStarshipRequest(
                     gameInfo.gameKey));
        return result?.Data?.starship;
    }

    public static async Task Set_ONE_DeviceDamage(GameData gameinfo, eDevice device, double damageLevel)
    {
        var starship = new StarshipDTO();
        starship.Devices.First(z => z.Name == device.ToString()).DamageLevel = damageLevel;
        await PostApi<DebugSetStarshipRequest, object>(
              gameinfo.client,
              "api/debug/set/starship",
              new DebugSetStarshipRequest(
                  gameinfo.gameKey,
                  starship));
    }

    public static async Task SetGalaxy(GameData gameInfo, double missionTime, double missionTimeDeadline)
    {
        await PostApi<DebugSetGalaxyRequest, object>(
            gameInfo.client,
            "api/debug/set/galaxy",
                new DebugSetGalaxyRequest(
                gameInfo.gameKey,
                missionTime,
                missionTimeDeadline));
    }

    public static async Task<(double? missionTime, double? missionTimeDeadline)> GetGalaxy(GameData gameInfo)
    {
        var result = await PostApi<
            DebugGetGalaxyRequest,
            DebugGetGalaxyResponse>(
                gameInfo.client,
                "api/debug/get/galaxy",
                new DebugGetGalaxyRequest(
                    gameInfo.gameKey));
        return (result?.Data?.missionTime, result?.Data?.missionTimeDeadline);
    }

    public static async Task ClearAllCQ(GameData gameInfo, bool all)
    {
        await PostApi<DebugSetClearCurrentQuadrantRequest, object>(
            gameInfo.client,
            "api/debug/set/clearcq",
                new DebugSetClearCurrentQuadrantRequest(
                gameInfo.gameKey,
                all));
    }

    public static async Task<List<SectorDTO>?> GetSectorsByContent(GameData gameInfo, string content)
    {
        var result = await PostApi<
            DebugGetSectorsByContentRequest,
            DebugGetSectorsByContentResponse>(
                gameInfo.client,
                "api/debug/get/sectors_by_content",
                new DebugGetSectorsByContentRequest(
                    gameInfo.gameKey,
                    content));
        return result?.Data?.sectors;
    }

    public static async Task SetStarshipQuadrant(GameData gameinfo, CoordDTO coord)
    {
        await PostApi<DebugSetStarshipQuadrantRequest, object>(
              gameinfo.client,
              "api/debug/set/starship_quadrant",
              new DebugSetStarshipQuadrantRequest(
                  gameinfo.gameKey,
                  coord));
    }

    public static async Task<CoordDTO?> GetStarshipQuadrant(GameData gameInfo)
    {
        var result = await PostApi<
             DebugGetStarshipQuadrantRequest,
             DebugGetStarshipQuadrantResponse>(
                 gameInfo.client,
                 "api/debug/get/starship_quadrant",
                 new DebugGetStarshipQuadrantRequest(
                     gameInfo.gameKey));
        return result?.Data?.coord;
    }

    public static async Task SetEnemyFireMove(GameData gameInfo, bool stopEnemyFire = false, bool skipEnemyMove = false, bool forceEnemyMove = false)
    {
        await PostApi<DebugSetEnemyFireMoveRequest, object>(
              gameInfo.client,
              "api/debug/set/enemy_fire_move",
              new DebugSetEnemyFireMoveRequest(
                  gameInfo.gameKey,
                  stopEnemyFire,
                  skipEnemyMove,
                  forceEnemyMove));

    }
}
