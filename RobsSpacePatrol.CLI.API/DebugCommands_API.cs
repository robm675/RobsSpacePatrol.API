using RobsSpacePatrol.CLI.API.Library;
using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Debug.Get;
using RobsSpacePatrol.DTO.Debug.Set;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;

namespace RobsSpacePatrol.CLI.API;

public class DebugCommands_API : IDebug
{
    private readonly GameData _cd;

    public DebugCommands_API(GameData cd)
    {
        _cd = cd;
    }

    private async Task<string> GetSectorContents(CoordDTO coord)
    {
        var req = new DebugGetSectorContentsRequest(_cd.GameID, coord);
        var resp = await _cd.PostForApiResponseAsync<DebugGetSectorContentsRequest, DebugGetSectorContentsResponse>("api/debug/get/sector_contents", req);
        return resp.Data?.SectorContents ?? throw new InvalidOperationException("The API returned no sector contents.");
    }

    private async Task<bool> SetSectorContents(CoordDTO coord, string contents)
    {
        var req = new DebugSetSectorContentsRequest(_cd.GameID, coord, contents);
        var resp = await _cd.PostForApiResponseAsync<DebugSetSectorContentsRequest, bool>("api/debug/set/sector_contents", req);
        return resp.Data;
    }

    private async Task<int> GetSectorEnemy(CoordDTO coord)
    {
        var req = new DebugGetEnemyRequest(_cd.GameID, coord);
        var resp = await _cd.PostForApiResponseAsync<DebugGetEnemyRequest, DebugGetEnemyResponse>("api/debug/get/enemy", req);
        return resp.Data?.enemy?.ShieldLevel ?? throw new InvalidOperationException("The API returned no enemy data.");
    }

    private async Task<bool> SetSectorEnemy(CoordDTO coord, int shieldLevel)
    {
        var req = new DebugSetEnemyRequest(_cd.GameID, coord, new EnemyDTO(shieldLevel));
        var resp = await _cd.PostForApiResponseAsync<DebugSetEnemyRequest, bool>("api/debug/set/enemy", req);
        return resp.Data;
    }

    private async Task<QuadrantDTO> GetQuadrant(CoordDTO coord)
    {
        var req = new DebugGetQuadrantRequest(_cd.GameID, coord);
        var resp = await _cd.PostForApiResponseAsync<DebugGetQuadrantRequest, DebugGetQuadrantResponse>("api/debug/get/quadrant", req);
        return resp.Data?.quadrant ?? throw new InvalidOperationException("The API returned no quadrant data.");
    }

    private async Task<bool> SetQuadrant(QuadrantDTO quadrant)
    {
        var req = new DebugSetQuadrantRequest(_cd.GameID, quadrant.Coord, quadrant);
        var resp = await _cd.PostForApiResponseAsync<DebugSetQuadrantRequest, bool>("api/debug/set/quadrant", req);
        return resp.Data;
    }

    private async Task<StarshipDTO> GetStarship()
    {
        var req = new DebugGetStarshipRequest(_cd.GameID);
        var resp = await _cd.PostForApiResponseAsync<DebugGetStarshipRequest, DebugGetStarshipResponse>("api/debug/get/starship", req);
        return resp.Data?.starship ?? throw new InvalidOperationException("The API returned no starship data.");
    }

    private async Task<bool> SetStarship(StarshipDTO starship)
    {
        var req = new DebugSetStarshipRequest(_cd.GameID, starship);
        var resp = await _cd.PostForApiResponseAsync<DebugSetStarshipRequest, bool>("api/debug/set/starship", req);
        return resp.Data;
    }

    private async Task<(double missionTime, double missionTimeDeadline)> GetGalaxy()
    {
        var req = new DebugGetGalaxyRequest(_cd.GameID);
        var resp = await _cd.PostForApiResponseAsync<DebugGetGalaxyRequest, DebugGetGalaxyResponse>("api/debug/get/galaxy", req);
        var galaxy = resp.Data ?? throw new InvalidOperationException("The API returned no galaxy data.");
        return (galaxy.missionTime, galaxy.missionTimeDeadline);
    }

    private async Task<bool> SetGalaxy(double missionTime, double missionTimeDeadline)
    {
        var req = new DebugSetGalaxyRequest(_cd.GameID, missionTime, missionTimeDeadline);
        var resp = await _cd.PostForApiResponseAsync<DebugSetGalaxyRequest, bool>("api/debug/set/galaxy", req);
        return resp.Data;
    }

    private async Task<bool> ClearCQ(bool allSW)
    {
        var req = new DebugSetClearCurrentQuadrantRequest(_cd.GameID, allSW);
        var resp = await _cd.PostForApiResponseAsync<DebugSetClearCurrentQuadrantRequest, bool>("api/debug/set/clearcq", req);
        return resp.Data;
    }

    private async Task<DebugGetSectorsByContentResponse> GetSectorsByContents(string contents)
    {
        var req = new DebugGetSectorsByContentRequest(_cd.GameID, contents);
        var resp = await _cd.PostForApiResponseAsync<DebugGetSectorsByContentRequest, DebugGetSectorsByContentResponse>("api/debug/get/sectors_by_content", req);
        return resp.Data ?? throw new InvalidOperationException("The API returned no sector data.");
    }

    private async Task<DebugGetStarshipQuadrantResponse> GetStarshipQuadrant()
    {
        var req = new DebugGetStarshipQuadrantRequest(_cd.GameID);
        var resp = await _cd.PostForApiResponseAsync<DebugGetStarshipQuadrantRequest, DebugGetStarshipQuadrantResponse>("api/debug/get/starship_quadrant", req);
        return resp.Data ?? throw new InvalidOperationException("The API returned no starship quadrant data.");
    }

    private async Task<bool> SetStarshipQuadrant(CoordDTO coord)
    {
        var req = new DebugSetStarshipQuadrantRequest(_cd.GameID, coord);
        var resp = await _cd.PostForApiResponseAsync<DebugSetStarshipQuadrantRequest, bool>("api/debug/set/starship_quadrant", req);
        return resp.Data;
    }

    private async Task<bool> SetEnemyFireMove(bool stopEnemyFire = false, bool skipEnemyMove = false, bool forceEnemyMove = false)
    {
        var req = new DebugSetEnemyFireMoveRequest(_cd.GameID, stopEnemyFire, skipEnemyMove, forceEnemyMove);
        var resp = await _cd.PostForApiResponseAsync<DebugSetEnemyFireMoveRequest, bool>("api/debug/set/enemy_fire_move", req);
        return resp.Data;
    }

    private CoordDTO getCoord(string x, string y)
    {
        if (!int.TryParse(x, out int _x))
            throw new Exception("Invalid x value: " + x);
        if (!int.TryParse(y, out int _y))
            throw new Exception("Invalid y value: " + y);
        return new CoordDTO(_x, _y);
    }



    public string runDEBUG(string[] commands)
    {
        switch (commands[1])
        {
            case "SET":
                return run_SET_DEBUG(commands).GetAwaiter().GetResult();

            case "GET":
                var res = run_GET_DEBUG(commands).GetAwaiter().GetResult();
                if (res == null) return CLI_Messages.DebugErrorInCommand;
                return res;

            case "CHECK":
                return CLI_Messages.DebugCommandOK;

            default:
                return CLI_Messages.DebugUnknownCommand;
        }
    }


    private async Task<string> run_SET_DEBUG(string[] commands)
    {
        if (commands.Count() < 3) return CLI_Messages.DebugInvalidCommand;
        switch (commands[2])
        {
            case "STOPENEMYFIRE":
                await SetEnemyFireMove(stopEnemyFire: true);
                return "OK";

            case "SKIPENEMYMOVE":
                switch (commands[3])
                {
                    case "ON":
                        if (await SetEnemyFireMove(skipEnemyMove: true))
                            return "OK";
                        else
                            return "ERROR";

                    case "OFF":
                        if (await SetEnemyFireMove(skipEnemyMove: false))
                            return "OK";
                        else
                            return "ERROR";

                    case "FORCE":
                        if (await SetEnemyFireMove(skipEnemyMove: false, forceEnemyMove: true))
                            return "OK";
                        else
                            return "ERROR";

                    default:
                        throw new Exception("Unknown command: " + string.Join('|', commands));
                }
            case "CLEARCQ":
                if (commands.Count() < 3) return CLI_Messages.DebugInvalidCommand;
                bool allSW = false;
                if (commands.Length >= 4 && commands[3] == "ALL") allSW = true;

                if (await ClearCQ(allSW))
                    return "OK";
                else
                    return "ERROR";

            case "CURRENTQUADRANT":
                if (commands.Count() < 4) return CLI_Messages.DebugInvalidCommand;
                switch (commands[3])
                {
                    case "SECTOR":
                        return await setSectorContents(commands);

                    case "ENEMY":
                        return await setSectorEnemy(commands);

                    default:
                        return CLI_Messages.DebugUnknownCommand;
                }
            case "QUADRANT":
                return await setQuadrantContents(commands);

            case "STARSHIP":
                return await setStarship(commands);

            case "GALAXY":
                return await setGalaxy(commands);

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }
    }

    private async Task<string> setSectorContents(string[] commands)
    {
        if (commands.Count() < 6) return CLI_Messages.DebugInvalidCommand;
        CoordDTO targetCoord = getCoord(commands[4], commands[5]);

        if (await SetSectorContents(targetCoord, commands[6].ToUpper().Trim()))
            return "OK";
        else
            return "ERROR";
    }

    private async Task<string> setSectorEnemy(string[] commands)
    {
        if (commands.Count() < 6) return CLI_Messages.DebugInvalidCommand;
        CoordDTO targetCoord = getCoord(commands[4], commands[5]);

        if (!int.TryParse(commands[6], out int shieldLevel)) throw new Exception("Invalid shield level given: " + commands[6]);

        if (await SetSectorEnemy(targetCoord, shieldLevel))
            return "OK";
        else
            return "ERROR";
    }

    private async Task<string> setQuadrantContents(string[] commands)
    {
        if (commands.Count() < 5) return CLI_Messages.DebugInvalidCommand;
        CoordDTO targetCoord = getCoord(commands[3], commands[4]);

        var quad = await GetQuadrant(targetCoord);

        if (!int.TryParse(commands[6], out int value)) throw new Exception("Invalid value given: " + commands[6]);

        switch (commands[5])
        {
            case "NUMENEMIES":
                quad.NumberOfEnemies = value;
                break;

            case "NUMSTARS":
                quad.NumberOfStars = value;
                break;

            case "STARBASE":
                if (value == 1)
                    quad.HasStarBase = true;
                else
                    quad.HasStarBase = false;
                break;

            case "EXPLORED":
                if (value == 1)
                    quad.HasBeenExplored = true;
                else
                    quad.HasBeenExplored = false;
                break;

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }

        if (await SetQuadrant(quad))
            return "OK";
        else
            return "ERROR";
    }

    private async Task<string> setStarship(string[] commands)
    {
        if (commands.Count() < 4) return CLI_Messages.DebugInvalidCommand;

        var ss = await GetStarship();

        switch (commands[3])
        {
            case "ENERGY":
                if (!int.TryParse(commands[4], out int energyLevel)) throw new Exception("Invalid value given: " + commands[4]);
                ss.EnergyLevel = energyLevel;
                break;

            case "SHIELD":
                if (!int.TryParse(commands[4], out int shield)) throw new Exception("Invalid value given: " + commands[4]);
                ss.ShieldLevel = shield;
                break;

            case "TORP":
                if (!int.TryParse(commands[4], out int torp)) throw new Exception("Invalid value given: " + commands[4]);
                ss.TorpedosRemaining = torp;
                break;

            case "DEVICE":
                if (!int.TryParse(commands[5], out int damageLvl)) throw new Exception("Invalid value given: " + commands[5]);

                var dev = ss.Devices.FirstOrDefault(z => z.Name == commands[4]);
                if (dev == null) throw new Exception("Invalid device name: " + commands[4]);

                dev.DamageLevel = damageLvl;
                break;

            case "QUADRANT":
                CoordDTO targetCoord = getCoord(commands[4], commands[5]);

                if (await SetStarshipQuadrant(targetCoord))
                    return "OK";
                else
                    return "ERROR";

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }

        if (await SetStarship(ss))
            return "OK";
        else
            return "ERROR";
    }

    private async Task<string> setGalaxy(string[] commands)
    {
        if (commands.Count() < 5) return CLI_Messages.DebugInvalidCommand;

        if (!double.TryParse(commands[4], out double value)) throw new Exception("Invalid value given: " + commands[5]);

        var tempGalaxy = await GetGalaxy();

        double missionTime = tempGalaxy.missionTime;
        double missionTimeDeadLine = tempGalaxy.missionTimeDeadline;

        switch (commands[3])
        {
            case "MISSIONTIME":
                missionTime = value;
                break;

            case "MISSIONTIMEDEADLINE":
                missionTimeDeadLine = value;
                break;

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }

        if (await SetGalaxy(missionTime, missionTimeDeadLine))
            return "OK";
        else
            return "ERROR";
    }


    private async Task<string?> run_GET_DEBUG(string[] commands)
    {
        if (commands.Count() < 3) return CLI_Messages.DebugInvalidCommand;

        switch (commands[2])
        {
            case "CURRENTQUADRANT":
                if (commands.Count() < 4) return CLI_Messages.DebugInvalidCommand;
                switch (commands[3])
                {
                    case "SECTOR":
                        return await getSectorContents(commands);

                    case "ENEMY":
                        return await getSectorEnemy(commands);

                    default:
                        return CLI_Messages.DebugUnknownCommand;
                }
            case "QUADRANT":
                return await getQuadrantContents(commands);

            case "STARSHIP":
                return await getStarship(commands);

            case "GALAXY":
                return await getGalaxy(commands);

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }
    }

    private async Task<string?> getSectorContents(string[] commands)
    {
        if (commands.Count() < 6) return CLI_Messages.DebugInvalidCommand;

        var targetCoord = getCoord(commands[4], commands[5]);
        var res2 = await GetSectorContents(targetCoord);

        return res2;
    }

    private async Task<string?> getSectorEnemy(string[] commands)
    {
        if (commands.Count() < 6) return CLI_Messages.DebugInvalidCommand;

        var targetCoord = getCoord(commands[4], commands[5]);
        var res = await GetSectorEnemy(targetCoord);

        return res.ToString();
    }

    private async Task<string> getQuadrantContents(string[] commands)
    {
        if (commands.Count() < 5) return CLI_Messages.DebugInvalidCommand;

        var targetCoord = getCoord(commands[3], commands[4]);
        var quad = await GetQuadrant(targetCoord);

        switch (commands[5])
        {
            case "NUMENEMIES":
                return quad.NumberOfEnemies.ToString();

            case "NUMSTARS":
                return quad.NumberOfStars.ToString();

            case "STARBASE":
                return quad.HasStarBase ? "1" : "0";

            case "EXPLORED":
                return quad.HasBeenExplored ? "1" : "0";

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }
    }

    private async Task<string> getStarship(string[] commands)
    {
        if (commands.Count() < 4) return CLI_Messages.DebugInvalidCommand;

        var ss = await GetStarship();

        switch (commands[3])
        {
            case "ENERGY":
                return ss.EnergyLevel.ToString();

            case "SHIELD":
                return ss.ShieldLevel.ToString();

            case "TORP":
                return ss.TorpedosRemaining.ToString();

            case "DEVICE":
                var dev = ss.Devices.FirstOrDefault(z => z.Name == commands[4]);
                if (dev == null) throw new Exception("Invalid device name: " + commands[4]);
                return dev.DamageLevel.ToString();

            case "SECTOR":
                var sectors = await GetSectorsByContents(RobsSpacePatrol.DTO.Library.Global.SectorStarship);
                if (sectors == null || sectors.sectors.Count != 1) throw new Exception("Invalid result from GetSectorsByContents looking for Starship");
                return sectors.sectors.First().Coord.ToString();

            case "QUADRANT":
                var quad = await GetStarshipQuadrant();
                return quad.coord.ToString();

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }
    }

    private async Task<string> getGalaxy(string[] commands)
    {
        if (commands.Count() < 4) return CLI_Messages.DebugInvalidCommand;

        var galaxy = await GetGalaxy();

        switch (commands[3])
        {
            case "MISSIONTIME":
                return galaxy.missionTime.ToString();

            case "MISSIONTIMEDEADLINE":
                return galaxy.missionTimeDeadline.ToString();

            default:
                throw new Exception("Unknown command: " + string.Join('|', commands));
        }
    }
}
