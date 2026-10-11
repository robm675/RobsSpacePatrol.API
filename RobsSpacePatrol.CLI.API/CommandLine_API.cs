using System.Net.Http.Json;
using RobsSpacePatrol.CLI.API.Library;
using RobsSpacePatrol.DTO;
using RobsSpacePatrol.DTO.Game;
using RobsSpacePatrol.DTO.Library;
using static RobsSpacePatrol.DTO.Library.Result;

namespace RobsSpacePatrol.CLI.API;

public class CommandLine_API : ICommandLine
{
    public bool GameOver { get; private set; }

    private GameData _cd;
    private DebugCommands_API _debug;
    private double? RepairMissionTimeUnits { get; set; }
    private bool newQuadrant = false;
    private bool quitSwitch = false;

    public CommandLine_API(GameData cd, DebugCommands_API debug)
    {
        _cd = cd;
        _debug = debug;
    }

    private string getSectorFormatted(SectorDTO tempSector)
    {
        switch (tempSector.SectorContents)
        {
            case Global.SectorEnemy:
                return "XXX";

            case Global.SectorStarship:
                return "SSS";

            case Global.SectorEmpty:
                return "   ";

            case Global.SectorStar:
                return "***";

            case Global.SectorStarBase:
                return "BBB";

            default:
                return "???";
        }
    }

    private string getSRSFormatted(List<SectorDTO> Sectors)
    {
        const string XHeader = "  <---------------------- X ---------------------->\n";
        const string Line = "+-----+-----+-----+-----+-----+-----+-----+-----+\n";
        if (Sectors == null) { return " "; }
        string OutStr = "";
        OutStr += XHeader;
        for (int y = 0; y < 8; y++)
        {
            switch (y)
            {
                case 0:
                    OutStr += "^ ";
                    OutStr += Line;
                    break;

                case 4:
                    OutStr += "Y " + Line;
                    break;

                default:
                    OutStr += "| ";
                    OutStr += Line;
                    break;
            }

            OutStr += "| ";
            for (int x = 0; x < 8; x++)
            {
                var CurSector = Sectors.Where(z => z.Coord.X == x && z.Coord.Y == y).FirstOrDefault();
                if (CurSector == null) continue;
                OutStr += "| " + getSectorFormatted(CurSector) + " ";
            }
            OutStr += "|\n";
        }
        OutStr += "v " + Line + "\n";
        return OutStr;
    }

    private string getQuadrantForGalaxyFormatted(QuadrantDTO Quadrant, bool MarkStarship = false)
    {
        string outstr = "";

        if (Quadrant == null) { return "| *** "; }
        if (!Quadrant.HasBeenExplored) { return "| *** "; }

        string ssMarkerStart = " ";
        string ssMarkerEnd = " ";

        if (MarkStarship)
        {
            ssMarkerStart = ">";
            ssMarkerEnd = "<";
        }


        outstr += "|" + ssMarkerStart;
        if (Quadrant.NumberOfEnemies > 0)
        {
            outstr += Quadrant.NumberOfEnemies;
        }
        else
        {
            outstr += Quadrant.NumberOfEnemies;
        }

        outstr += Quadrant.HasStarBase ? "1" : "0";

        outstr += Quadrant.NumberOfStars;
        outstr += ssMarkerEnd;

        return outstr;
    }

    private string getLRSFormatted(List<QuadrantDTO> Quadrants, CoordDTO ssQuadCoord)
    {
        Quadrants = Quadrants.Where(z => z != null).ToList();

        const string XHeader = "  <------- X ------->\n";
        const string Line = "+-----+-----+-----+\n";
        string OutStr = "";
        OutStr += XHeader;
        OutStr += "^ " + Line;
        for (int y = ssQuadCoord.Y - 1; y <= ssQuadCoord.Y + 1; y++)
        {
            if (y != ssQuadCoord.Y - 1)
            {
                OutStr += "| " + Line;
            }

            if (y == ssQuadCoord.Y)
            {
                OutStr += "Y ";
            }
            else
            {
                OutStr += "| ";
            }

            for (int x = ssQuadCoord.X - 1; x <= ssQuadCoord.X + 1; x++)
            {
                if (y < 0 || y > 7 || x < 0 || x > 7)
                {
                    OutStr += "| *** ";
                }
                else
                {
                    var CurQuad = Quadrants.Where(z => z.Coord != null && z.Coord.X == x && z.Coord.Y == y).FirstOrDefault();
                    if (CurQuad == null) continue;
                    OutStr += getQuadrantForGalaxyFormatted(CurQuad);
                }
            }
            OutStr += "|\n";
        }
        OutStr += "v " + Line;
        return OutStr;
    }

    private string showHelpNavTor(bool isNav)
    {
        string stringOut = string.Empty;

        stringOut += "\n";
        stringOut += "Direction:\n\n";
        stringOut += "  4    3    2\n";
        stringOut += "   `.  :  .'\n";
        stringOut += "     `.:.'\n";
        stringOut += "  5---<*>---1\n"; ;
        stringOut += "     .':`.\n";
        stringOut += "   .'  :  `.\n";
        stringOut += "  6    7    8\n";

        if (isNav)
        {
            stringOut += "\n";
            stringOut += "Space-drive speed 1 to 8, impulse power 0.1 to 0.9";
        }


        return stringOut;
    }

    private string showHelpComputer()
    {
        string stringOut = string.Empty;

        stringOut += "\n";
        stringOut += "COM REC\t\t\t - Computer, Galactic Record\n";
        stringOut += "COM TOR\t\t\t - Computer, Torpedo calculator, shows direction to each enemy in quadrant\n";
        stringOut += "COM NAV ##.## $$.$$\t - Computer, Navigation calculator, ##.## destination quadrant, $$.$$ destination sector\n";
        stringOut += "COM STB\t\t\t - Computer, Starbase navigation calculator, shows direction and distance to starbase\n";
        stringOut += "\t\t\t   in this quadrant\n";
        stringOut += "COM STA\t\t\t - Computer, Status report\n";

        return stringOut;
    }

    private string getGalaxyFormatted(List<QuadrantDTO> quadrants, CoordDTO currentQuadCoord)
    {
        string outstr = "";

        const string XHeader = "  <---------------------- X ---------------------->\n";
        const string Line = "+-----+-----+-----+-----+-----+-----+-----+-----+\n";

        outstr += XHeader;
        for (int y = 0; y <= 7; y++)
        {
            switch (y)
            {
                case 0:
                    outstr += "^";
                    break;

                case 4:
                    outstr += "Y";
                    break;

                default:
                    outstr += "|";
                    break;
            }
            outstr += " " + Line;
            outstr += "| ";
            for (int x = 0; x <= 7; x++)
            {
                var QUAD = quadrants.SingleOrDefault(z => z.Coord.isSame(new CoordDTO(x, y)));
                if (QUAD == null) continue;
                if (currentQuadCoord.isSame(new CoordDTO(x, y)))
                {
                    outstr += getQuadrantForGalaxyFormatted(QUAD, true);
                }
                else
                {
                    outstr += getQuadrantForGalaxyFormatted(QUAD);
                }
            }
            outstr += "|\n";
        }
        outstr += "v " + Line;
        return outstr;
    }

    private string showHelpMain()
    {
        string stringOut = string.Empty;

        stringOut += "SRS\t\t - Short Range Sensors - Shows what's in the current quadrant\n";
        stringOut += "LRS\t\t - Long Range Sensors - Shows what's in the adjacent quadrants\n";
        stringOut += "SHE ###\t\t - Shield Control - Set the shield level to ###\n";
        stringOut += "LAS ###\t\t - Laser Control - Fire Lasers ### amount\n";
        stringOut += "TOR ###\t\t - Torpedo Control - Fire torpedos in ### direction\n";
        stringOut += "COM ###\t\t - Computer Control - Run ### computer command\n";
        stringOut += "NAV ### $$$\t - Navigation Control - move ship in ### direction $$$ distance\n";
        stringOut += "QUIT\t\t - End the game\n";
        stringOut += "LSG\t\t - Lists existing save games\n";
        stringOut += "SAVEGAME XXX\t - Save the game with XXX description\n";
        stringOut += "LOADGAME XXX\t - Load the game with XXX description\n";
        stringOut += "DAM\t\t - Show damages\n";
        stringOut += "HELP\t\t - Show this screen\n";

        return stringOut;
    }



    private async Task<ApiResponse<SRS_Response>> GetSRS()
    {
        var srsRequest = new SRS_Request(_cd.GameID);
        return await _cd.PostForApiResponseAsync<SRS_Request, SRS_Response>("api/game/srs", srsRequest);
    }

    private async Task<ApiResponse<LRS_Response>> GetLRS()
    {
        var lrsRequest = new LRS_Request(_cd.GameID);
        return await _cd.PostForApiResponseAsync<LRS_Request, LRS_Response>("api/game/lrs", lrsRequest);
    }

    private async Task<ApiResponse<COM_NAV_Response>> GetCOM_NAV(CoordDTO quadCoord, CoordDTO sectCoord)
    {
        var comnavRequest = new COM_NAV_Request(_cd.GameID, quadCoord, sectCoord);
        return await _cd.PostForApiResponseAsync<COM_NAV_Request, COM_NAV_Response>("api/game/com_nav", comnavRequest);
    }

    private async Task<ApiResponse<COM_REC_Response>> GetCOM_REC()
    {
        var comrecRequest = new COM_REC_Request(_cd.GameID);
        return await _cd.PostForApiResponseAsync<COM_REC_Request, COM_REC_Response>("api/game/com_rec", comrecRequest);
    }

    private async Task<ApiResponse<COM_STA_Response>> GetCOM_STA()
    {
        var req = new COM_STA_Request(_cd.GameID);
        return await _cd.PostForApiResponseAsync<COM_STA_Request, COM_STA_Response>("api/game/com_sta", req);
    }

    private async Task<ApiResponse<COM_STB_Response>> GetCOM_STB()
    {
        var req = new COM_STB_Request(_cd.GameID);
        return await _cd.PostForApiResponseAsync<COM_STB_Request, COM_STB_Response>("api/game/com_stb", req);
    }

    private async Task<ApiResponse<COM_TOR_Response>> GetCOM_TOR()
    {
        var req = new COM_TOR_Request(_cd.GameID);
        return await _cd.PostForApiResponseAsync<COM_TOR_Request, COM_TOR_Response>("api/game/com_tor", req);
    }

    private async Task<ApiResponse<DAM_Response>> GetDAM()
    {
        var req = new DAM_Request(_cd.GameID);
        return await _cd.PostForApiResponseAsync<DAM_Request, DAM_Response>("api/game/dam", req);
    }

    private async Task<ApiResponse<NAV_Response>> GetNAV(double dir, double dist)
    {
        var req = new NAV_Request(_cd.GameID, dir, dist);
        return await _cd.PostForApiResponseAsync<NAV_Request, NAV_Response>("api/game/nav", req);
    }

    private async Task<ApiResponse<LAS_Response>> GetLAS(int energy)
    {
        var req = new LAS_Request(_cd.GameID, energy);
        return await _cd.PostForApiResponseAsync<LAS_Request, LAS_Response>("api/game/las", req);
    }

    private async Task<ApiResponse<SHE_Response>> GetSHE(int energy)
    {
        var req = new SHE_Request(_cd.GameID, energy);
        return await _cd.PostForApiResponseAsync<SHE_Request, SHE_Response>("api/game/she", req);
    }

    private async Task<ApiResponse<TOR_Response>> GetTOR(double dir)
    {
        var req = new TOR_Request(_cd.GameID, dir);
        return await _cd.PostForApiResponseAsync<TOR_Request, TOR_Response>("api/game/tor", req);
    }

    private async Task<string> runSRS(string[] commands)
    {
        var resp = await GetSRS();

        if (resp.Damaged) return CLI_Messages.SRS_Damaged;
        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;
        if (resp.Data == null || resp.Data.maintResult == null) throw new Exception("SRS returned null");

        var data = resp.Data.sectors;

        if (data == null) throw new Exception("SRS returned null");
        return getSRSFormatted(data) + getRoutineMaintenance(resp.Data.maintResult);
    }

    private async Task<string> runLRS(string[] Commands)
    {
        var resp = await GetLRS();

        if (resp.Damaged) return CLI_Messages.LRS_Damaged;
        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        return getLRSFormatted(resp.Data.quadrants, resp.Data.starshipQuadrantCoord) + getRoutineMaintenance(resp.Data.maintResult);
    }

    private async Task<string> runSHE(string[] Commands)
    {
        if (Commands.Length == 1) return CLI_Messages.SHE_MissingAmount;
        if (!int.TryParse(Commands[1], out int newShieldLevel)) { return CLI_Messages.SHE_InvalidAmount; }

        var resp = await GetSHE(newShieldLevel);

        if (resp.Damaged) return CLI_Messages.SHE_Damaged;

        if (!resp.Success) return CLI_Messages.SHE_InvalidAmount;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        string msg = CLI_Messages.SHE_Changed + newShieldLevel.ToString();
        return msg + getRoutineMaintenance(resp.Data.maintResult);
    }

    private async Task<string> runLAS(string[] Commands)
    {
        if (Commands.Length == 1) return CLI_Messages.LAS_MissingAmount;

        if (!int.TryParse(Commands[1], out int LaserEnergyAmount)) return CLI_Messages.LAS_InvalidAmount;

        var resp = await GetLAS(LaserEnergyAmount);

        if (resp.Damaged) return CLI_Messages.LAS_Damaged;

        if (resp.InfoOnly && resp.Message == GlobalVars.NoEnemiesFound) return CLI_Messages.LAS_NoEnemies;

        if (resp.InfoOnly && resp.Message == GlobalVars.InsufficientEnergy) return CLI_Messages.LAS_InsufficientEnergy;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        string OutMsg = string.Empty;
        foreach (var objHit in resp.Data.objectHitReports)
        {
            string objHitCoord = $"{objHit.Coord!.X}.{objHit.Coord.Y}";
            if (objHit.Destroyed)
            {
                if (OutMsg != string.Empty) OutMsg += "\n";
                OutMsg += CLI_Messages.LAS_Fired + objHit.UnitHit + CLI_Messages.LAS_Hit + objHitCoord + CLI_Messages.LAS_EnemyDestroyed;
            }
            else
            {
                if (OutMsg != string.Empty) OutMsg += "\n";
                OutMsg += CLI_Messages.LAS_Fired + objHit.UnitHit + CLI_Messages.LAS_Hit + objHitCoord + CLI_Messages.LAS_SensorsIndicate + objHit.ShieldRemaining + CLI_Messages.LAS_Remaining;
            }
        }

        var allDestroyed = resp.Data.objectHitReports.All(z => z.Destroyed);
        var lowestShieldLevel = resp.Data.objectHitReports.Min(z => z.ShieldRemaining);

        return OutMsg + getRoutineMaintenance(resp.Data.maintResult);
    }

    private async Task<string> runTOR(string[] Commands)
    {
        if (Commands.Length == 1)
            return CLI_Messages.TOR_MissingDirection + showHelpNavTor(false);

        if (!double.TryParse(Commands[1], out double DIR))
            return CLI_Messages.TOR_InvalidDirection + Commands[1];

        if (DIR < 0 || DIR >= 9)
            return CLI_Messages.TOR_InvalidDirection + showHelpNavTor(false);

        var resp = await GetTOR(DIR);

        if (resp.Damaged) return CLI_Messages.TOR_Damaged;


        if (resp.InfoOnly && resp.Message == GlobalVars.NoEnemiesFound) return CLI_Messages.TOR_NoEnemies;

        if (resp.InfoOnly && resp.Message == GlobalVars.InsufficientInventory) return CLI_Messages.TOR_Expended;

        if (resp.InfoOnly && resp.Message == GlobalVars.TorpMissed) return CLI_Messages.TOR_Missed;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        string torOutMsg = string.Empty;

        switch (resp.Data.objectHitReport.ObjectHit)
        {
            case eObjectHit.Enemy:
                torOutMsg += CLI_Messages.TOR_Fired + CLI_Messages.TOR_EnemyAtSector + resp.Data.objectHitReport.Coord.ToString() + CLI_Messages.TOR_Destroyed;
                break;

            case eObjectHit.Star:
                if (resp.Data.objectHitReport.Destroyed)
                {
                    torOutMsg += CLI_Messages.TOR_Fired + CLI_Messages.TOR_StarDestroyed;
                }
                else
                {
                    torOutMsg += CLI_Messages.TOR_Fired + CLI_Messages.TOR_StarAbsorbedTorp;
                }
                break;

            case eObjectHit.Starbase:
                torOutMsg += CLI_Messages.TOR_Fired + CLI_Messages.TOR_StarBaseDestroyed;
                break;
        }

        return torOutMsg + getRoutineMaintenance(resp.Data.maintResult);
    }

    private async Task<string> runCOM(string[] Commands)
    {
        if (Commands.Length < 2)
        {
            return CLI_Messages.CPU_InvalidCommand + showHelpComputer();
        }
        switch (Commands[1].ToUpper().Trim())
        {
            case "REC":
                return await runCOM_REC(Commands);

            case "TOR":
                return await runCOM_TOR(Commands);

            case "NAV":
                return await runCOM_NAV(Commands);

            case "STB":
                return await runCOM_STB(Commands);

            case "STA":
                return await runCOM_STA(Commands);

            default:
                return CLI_Messages.CPU_InvalidCommand;
        }
    }

    private async Task<string> runCOM_REC(string[] Commands)
    {
        var resp = await GetCOM_REC();
        if (resp.Damaged) return CLI_Messages.CPU_Damaged;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        return getGalaxyFormatted(resp.Data.quadrants, resp.Data.currentQuadCoord);
    }

    private async Task<string> runCOM_TOR(string[] Commands)
    {
        var resp = await GetCOM_TOR();
        if (resp.Damaged) return CLI_Messages.CPU_Damaged;


        if (resp.InfoOnly && resp.Message == GlobalVars.NoEnemiesFound) return CLI_Messages.CPU_TOR_NoEnemies;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        string cpuOutMsg = string.Empty;
        foreach (var DTE in resp.Data.directionToEnemies)
        {
            if (cpuOutMsg != string.Empty) { cpuOutMsg += "\n"; }
            string coord = DTE.Coord.X + "." + DTE.Coord.Y;
            cpuOutMsg += CLI_Messages.CPU_TOR_EnemyAtSector + coord + CLI_Messages.CPU_TOR_IsBearing + DTE.Direction.ToString("0.0");
        }

        return cpuOutMsg;
    }

    private async Task<string> runCOM_NAV(string[] Commands)
    {
        if (Commands.Length < 5)
            return CLI_Messages.CPU_NAV_Invalid + showHelpComputer();

        if (!int.TryParse(Commands[2], out var QX))
            return CLI_Messages.CPU_NAV_InvalidQX;

        if (QX < 0 || QX > 8)
            return CLI_Messages.CPU_NAV_InvalidCoord;

        if (!int.TryParse(Commands[3], out var QY))
            return CLI_Messages.CPU_NAV_InvalidQY;

        if (QY < 0 || QY > 8)
            return CLI_Messages.CPU_NAV_InvalidCoord;

        if (!int.TryParse(Commands[4], out var SX))
            return CLI_Messages.CPU_NAV_InvalidSX;

        if (SX < 0 || SX > 8)
            return CLI_Messages.CPU_NAV_InvalidCoord;

        if (!int.TryParse(Commands[5], out var SY))
            return CLI_Messages.CPU_NAV_InvalidSY;

        if (SY < 0 || SY > 8)
            return CLI_Messages.CPU_NAV_InvalidCoord;

        var resp = await GetCOM_NAV(new CoordDTO(QX, QY), new CoordDTO(SX, SY));

        if (resp.Damaged) return CLI_Messages.CPU_Damaged;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        return CLI_Messages.CPU_NAV_Dir + resp.Data.dir + CLI_Messages.CPU_NAV_Dist + resp.Data.dist;
    }

    private async Task<string> runCOM_STB(string[] Commands)
    {
        var resp = await GetCOM_STB();
        if (resp.Damaged) return CLI_Messages.CPU_Damaged;

        if (resp.InfoOnly && resp.Message == GlobalVars.StarbaseNotFound) return CLI_Messages.CPU_STB_NoStarBases;

        if (resp.Data == null)
            return CLI_Messages.InternalErrorMessage;

        return CLI_Messages.CPU_STB_StarbaseFoundInSector +
            resp.Data.starbaseCoord +
            "] " +
            CLI_Messages.CPU_STB_Dir +
            resp.Data.dir.ToString("0.0") +
            CLI_Messages.CPU_STB_Dist +
            resp.Data.dist.ToString("0.0");
    }

    private async Task<string> runCOM_STA(string[] Commands)
    {
        var resp = await GetCOM_STA();
        if (resp.Damaged) return CLI_Messages.CPU_Damaged;

        string cpuStaOutMsg = string.Empty;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        cpuStaOutMsg += CLI_Messages.CPU_STA_MissionTime + resp.Data.missionTime.ToString("0000.0") + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_Quadrant + resp.Data.quadrantCoord + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_Sector + resp.Data.sectorCoord + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_DevicesDamaged + "\n";
        foreach (var device in resp.Data.allDevices)
        {
            if (device == null) continue;
            if (device.Name == null) continue;

            var msg = string.Empty;
            if (device.DamageLevel < 0)
                msg = "Damaged - " + Math.Abs(device.DamageLevel).ToString("#.0") + (Math.Abs(device.DamageLevel) == 1 ? " mission time unit" : " mission time units");
            else
                msg = "Fully Functional";

            cpuStaOutMsg += "\t" + device.Name.PadRight(30) + msg + "\n";
        }
        cpuStaOutMsg += CLI_Messages.CPU_STA_EnemiesRemaining + resp.Data.enemiesRemaining + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_StarbasesRemaining + resp.Data.starBaseRemaining + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_EnergyRemaining + resp.Data.energyRemaining + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_ShieldLevel + resp.Data.shieldLevel + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_TorpedoRemaining + resp.Data.torpedosRemaining + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_Docked + (resp.Data.docked ? "Docked" : "Not Docked") + "\n";
        cpuStaOutMsg += CLI_Messages.CPU_STA_MissionTimeUnitsRemaining + (resp.Data.missionTimeDeadLine - resp.Data.missionTime).ToString("#.0") + "\n";

        return cpuStaOutMsg;
    }

    private async Task<string> runDAM(string[] Commands)
    {
        var resp = await GetDAM();

        string damOutMsg = string.Empty;
        damOutMsg += CLI_Messages.DAM_DamageReport + "\n";

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        foreach (var device in resp.Data.devices)
        {
            if (device == null || device.Name == null)
                return CLI_Messages.InternalErrorMessage;

            string Msg = (device.DamageLevel < 0 ? "Damaged - " + Math.Abs(device.DamageLevel).ToString("0.0") + (Math.Abs(device.DamageLevel) == 1 ? " mission time unit" : " mission time units") : "Fully Functional");
            damOutMsg += "\t" + device.Name.PadRight(30) + Msg + "\n";
        }
        if (resp.Data.devices.Count == 0)
            damOutMsg += $"\t{CLI_Messages.DAM_DamageReport_NoDamages}\n";

        return damOutMsg;
    }

    private async Task<string> runNAV(string[] Commands)
    {
        if (Commands.Length != 3)
            return CLI_Messages.NAV_InvalidCommand + showHelpNavTor(true);

        if (!double.TryParse(Commands[1], out double DIR))
            return CLI_Messages.NAV_InvalidDIRValue + Commands[1];
        if (DIR < 0 || DIR >= 9)
            return CLI_Messages.NAV_Invalid_DIR + DIR + showHelpNavTor(true);

        if (!double.TryParse(Commands[2], out double DIST))
            return CLI_Messages.NAV_InvalidDISTValue + Commands[2];
        if (DIST < 0 || DIST > 8)
            return CLI_Messages.NAV_Invalid_DIST + DIST + showHelpNavTor(true);

        var resp = await GetNAV(DIR, DIST);

        if (resp.Damaged) return CLI_Messages.NAV_SpaceDriveDamaged;

        if (resp.Data == null) return CLI_Messages.InternalErrorMessage;

        if (!resp.Data.startQuadCoord.isSame(resp.Data.finalQuadCoord))
            newQuadrant = true;

        if (resp.Data.navMessage == eNavMessage.InsufficientEnergy_ShieldEnergyAvailable) return CLI_Messages.NAV_NotEnoughEnergy_ShieldEnergyAvail;

        if (resp.Data.navMessage == eNavMessage.BadInput_OutsideGalaxy) return CLI_Messages.NAV_OutsideGalaxy;

        if (resp.Data.navMessage == eNavMessage.BadInput_ObjectHit)
            return CLI_Messages.NAV_ImpulseEnginesShutDown + resp.Data.finalSectCoord.ToString() + CLI_Messages.NAV_BadNavigation;



        var rmMaintMsg = getRoutineMaintenance(resp.Data.maintResult);

        if (resp.Data.navMessage == eNavMessage.TransitComplete && DIST < 1)
            return CLI_Messages.NAV_ImpulseEngaged + rmMaintMsg;

        if (resp.Data.navMessage == eNavMessage.TransitComplete && DIST >= 1)
            return CLI_Messages.NAV_SpaceDriveEngaged + rmMaintMsg;

        return string.Empty;
    }




    private string getRoutineMaintenance(Result.MaintResult data)
    {
        string OutString = "\n";

        if (data == null) return String.Empty;
        if (data.DockingStatus == null) return CLI_Messages.InternalErrorMessage;

        if (data.GameStatus != eGameStatus.Normal)
        {
            GameOver = true;
            switch (data.GameStatus)
            {
                case eGameStatus.StarshipDestroyed:
                    return CLI_Messages.END_MissionFailed_StarshipDestroyed;

                case eGameStatus.RanOutOfEnergy:
                    return CLI_Messages.END_MissionFailed_RanOutOfEnergy;

                case eGameStatus.RanOutOfTime:
                    return CLI_Messages.END_MissionFailed_RanOutOfTime;

                case eGameStatus.OutOfEnergyShieldEnergyAvailable:
                    OutString += CLI_Messages.END_EnergyGoneShieldEnergyAvail;
                    break;

                case eGameStatus.MissionOver:
                    return "\n" + CLI_Messages.END_MissionAccomplished;

                default:
                    break;
            }
        }


        if (!data.DockingStatus.PreviousStatus && data.DockingStatus.CurrentStatus)
        {
            OutString += CLI_Messages.MAINT_Docked + "\n";
        }

        if (data.StarbaseRepairsMissionTimeUnits != null)
        {
            OutString += CLI_Messages.MAINT_TechWaiting + CLI_Messages.MAINT_ItWillTake + Math.Abs(data.StarbaseRepairsMissionTimeUnits.Value) + CLI_Messages.MAINT_MissionTimeUnits + "\n" + CLI_Messages.MAINT_ReplyYES;
            RepairMissionTimeUnits = data.StarbaseRepairsMissionTimeUnits;
        }

        if (data.DevicesRepairStatusChanged != null)
        {
            foreach (var Device in data.DevicesRepairStatusChanged)
            {
                if (Device.DamageLevel == 100)
                {
                    OutString += CLI_Messages.MAINT_RepairsComplete + Device.Name + "\n";

                    Device.DamageLevel = 0;
                }
            }
        }

        bool StarshipDestroyedMessageSent = false;

        if (data.EnemiesMoved != null)
        {
            foreach (var enemy in data.EnemiesMoved)
            {
                string OutLine = string.Empty;
                string msg = string.Format(CLI_Messages.MAINT_EnemyMoved, enemy.EnemyCoord_Orig, enemy.EnemyCoord_Dest);
                OutLine += msg;
                OutString += "\n" + OutLine;
            }
        }

        if (data.EnemiesFired != null)
        {
            string OutLine = string.Empty;

            foreach (var EnemyFired in data.EnemiesFired)
            {
                if (StarshipDestroyedMessageSent) { continue; }
                if (OutLine.Trim() != string.Empty) { OutLine += "\n"; }

                if (!data.DockingStatus.CurrentStatus)
                {
                    if (EnemyFired.StarshipDestroyed)
                    {
                        OutLine += string.Format(CLI_Messages.MAINT_EnemyFiredStarshipDestroyed, EnemyFired.EnemyFiredAmount, EnemyFired.EnemyCoord);
                        StarshipDestroyedMessageSent = true;
                    }
                    else
                    {
                        OutLine += string.Format(CLI_Messages.MAINT_EnemyFiredShieldHit, EnemyFired.EnemyFiredAmount, EnemyFired.EnemyCoord, EnemyFired.StarshipNewShieldAmount);
                        if (EnemyFired.DeviceDamaged != null)
                        {
                            OutLine += "\n" + EnemyFired.DeviceDamaged.Name + CLI_Messages.MAINT_DeviceDamaged + Math.Round(Math.Abs(EnemyFired.DeviceDamangedAmount), 1) + CLI_Messages.MAINT_MissionTimeUnitsToRepair;
                        }
                    }
                }
                else
                {
                    OutLine += string.Format(CLI_Messages.MAINT_EnemyFiredProtected, EnemyFired.EnemyFiredAmount, EnemyFired.EnemyCoord, 0) + CLI_Messages.MAINT_StarBaseShieldsProtected;
                }
            }
            OutString += "\n" + OutLine;
        }


        if (!data.DockingStatus.CurrentStatus && !StarshipDestroyedMessageSent)
        {
            if (data.StarshipShieldLevel < 50 && data.AnyEnemiesInQuadrant)
                OutString += "\n" + CLI_Messages.WarningMessageShieldDownInCombatArea;
        }
        if (newQuadrant)
        {
            OutString += "\n" + CLI_Messages.MAINT_NewQuadrantEnemiesRedAlert;
            newQuadrant = false;
        }

        return OutString;
    }

    //there aren't any tests to verify routine maint ran after a successful command - except for SRS
    //there aren't any tests to verify routine maint ran after a successful command - except for SRS
    //there aren't any tests to verify routine maint ran after a successful command - except for SRS

    public string ExecuteCommandString(string CommandLine)
    {
        if (CommandLine.Trim() == string.Empty) { return ""; }

        string[] Commands = CommandLine.Split(new char[] { ' ', '[', ']', ',', '=', ':' }, StringSplitOptions.RemoveEmptyEntries);
        string Command = Commands[0].ToUpper().Trim();
        string OutString = string.Empty;


        switch (Command.Trim())
        {
            case "YES":
                if (quitSwitch)
                {
                    OutString += CLI_Messages.END_MissionFailed_Quit;
                    GameOver = true;
                }
                break;

            case "SRS":
                return runSRS(Commands).GetAwaiter().GetResult();

            case "LRS":
                return runLRS(Commands).GetAwaiter().GetResult();

            case "SHE":
                return runSHE(Commands).GetAwaiter().GetResult();

            case "LAS":
                return runLAS(Commands).GetAwaiter().GetResult();

            case "TOR":
                return runTOR(Commands).GetAwaiter().GetResult();

            case "COM":
                return runCOM(Commands).GetAwaiter().GetResult();

            case "NAV":
                return runNAV(Commands).GetAwaiter().GetResult();

            case "DAM":
                return runDAM(Commands).GetAwaiter().GetResult();

            case "DEBUG":
                return _debug.runDEBUG(Commands);

            case "QUIT":
                OutString += "Are you abandoning your mission? (YES/NO)";
                quitSwitch = true;
                break;

            case "HELP":
                return showHelpMain();

            default:
                if (Command.Trim() != "")
                {
                    OutString += $"{CLI_Messages.UnknownCommand}";
                    OutString += "\n";
                    OutString += showHelpMain();
                    OutString += "\n";
                }
                return OutString;
        }
        return OutString;
    }
}