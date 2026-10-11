namespace RobsSpacePatrol.CLI.API.Library;

public class CLI_Messages
{
    public const string ErrorMessage = "**ERROR**";
    public const string InternalErrorMessage = "**Internal Error Has Occurred**";

    public const string UnknownCommand = "Unknown Command: ";

    public const string DebugCommandOK = "OK";
    public const string DebugUnknownCommand = "Unknown Debug Command";
    public const string DebugErrorInCommand = "There was an error in the debug command";
    public const string DebugInvalidCommand = "Invalid debug command or parameter";

    public const string WarningMessageShieldDownInCombatArea = "Warning! Shields dangerously low in a combat area!";

    public const string GameSavedGood = "Game successfully saved.";
    public const string GameSavedError = "There was a problem saving the game.";
    public const string LoadSaveGame = "Game loaded.";
    public const string LoadSaveGameDisabled = "Loading and Saving games has been disabled.";


    public const string DAM_DamageReport = "Damage Report:";
    public const string DAM_DamageReport_NoDamages = "No damages - All devices functional";

    public const string SRS_Damaged = "Short Range Sensors Damaged";

    public const string LRS_Damaged = "Long Range Sensors Damaged";

    public const string SHE_MissingAmount = "Invalid Shield Command - Missing Amount";
    public const string SHE_InvalidAmount = "Invalid SHE Amount: ";
    public const string SHE_Changed = "Shield level changed to: ";
    public const string SHE_Damaged = "Shield Control Damaged";

    public const string LAS_MissingAmount = "Invalid LAS Command - Missing Amount";
    public const string LAS_InvalidAmount = "Invalid LAS Amount: ";
    public const string LAS_Fired = "Lasers Fired ";
    public const string LAS_Hit = " hit on enemy in sector [";
    public const string LAS_EnemyDestroyed = "] Enemy destroyed! ";
    public const string LAS_SensorsIndicate = "] (Sensors indicate ";
    public const string LAS_Remaining = " remaining)";
    public const string LAS_Damaged = "Laser Control Damaged";
    public const string LAS_InsufficientEnergy = "Insufficient Energy";
    public const string LAS_NoEnemies = "No Enemies in Quadrant";

    public const string TOR_MissingDirection = "Invalid TOR Command - Missing Direction";
    public const string TOR_InvalidDirection = "Invalid TOR Direction: ";
    public const string TOR_Fired = "Torpedo Fired! ";
    public const string TOR_EnemyAtSector = "Enemy at sector [";
    public const string TOR_Destroyed = "] destroyed!";
    public const string TOR_Damaged = "Torpedo Control Damaged";
    public const string TOR_Expended = "All Torpedos Expended";
    public const string TOR_Missed = "Torpedo Missed! ";
    public const string TOR_NoEnemies = "No enemies found in quadrant";
    public const string TOR_StarDestroyed = "Star destroyed! ";
    public const string TOR_StarAbsorbedTorp = "Star absorbed Torpedo! ";
    public const string TOR_StarBaseDestroyed = "StarBase destroyed! ";

    public const string CPU_Damaged = "Computer Damaged";
    public const string CPU_NAV_Invalid = "Invalid CPU NAV command";  // NEED TO TEST FOR
    public const string CPU_NAV_InvalidQX = "Invalid CPU NAV Quadrant X: ";
    public const string CPU_NAV_InvalidQY = "Invalid CPU NAV Quadrant Y: ";
    public const string CPU_NAV_InvalidSX = "Invalid CPU NAV Sector X: ";
    public const string CPU_NAV_InvalidSY = "Invalid CPU NAV Sector Y: ";
    public const string CPU_NAV_InvalidCoord = "Invalid Target Coordinates";
    public const string CPU_NAV_Dir = "Direction: ";
    public const string CPU_NAV_Dist = " Distance: ";
    public const string CPU_TOR_EnemyAtSector = "Enemy at sector [";
    public const string CPU_TOR_IsBearing = "] is bearing ";
    public const string CPU_TOR_NoEnemies = "No enemies in sector.";
    public const string CPU_STB_NoStarBases = "No starbases in quadrant.";
    public const string CPU_STB_StarbaseFoundInSector = "Starbase found in sector [";
    public const string CPU_STB_Dir = "Direction: ";
    public const string CPU_STB_Dist = " Distance: ";
    public const string CPU_InvalidCommand = "Invalid CPU command"; // NEED TO TEST FOR
    public const string CPU_STA_MissionTime = "Mission time: \t\t";
    public const string CPU_STA_Quadrant = "Quadrant: \t\t";
    public const string CPU_STA_Sector = "Sector: \t\t";
    public const string CPU_STA_DevicesDamaged = "Device Status:";
    public const string CPU_STA_EnemiesRemaining = "Enemies Remaining:\t";
    public const string CPU_STA_StarbasesRemaining = "Starbases Remaining:\t";
    public const string CPU_STA_EnergyRemaining = "Energy:\t\t\t";
    public const string CPU_STA_ShieldLevel = "Shields:\t\t";
    public const string CPU_STA_TorpedoRemaining = "Torpedos:\t\t";
    public const string CPU_STA_Docked = "Docked Status:\t\t";
    public const string CPU_STA_MissionTimeUnitsRemaining = "Mission time remaining:\t";

    public const string NAV_InvalidCommand = "Invalid NAV command";
    public const string NAV_InvalidDIRValue = "Invalid NAV DIR value: ";
    public const string NAV_InvalidDISTValue = "Invalid NAV DIST value: ";
    public const string NAV_OutsideGalaxy = "Space drive cancelled - Navigation outside the galaxy is not permitted.";
    public const string NAV_SpaceDriveDamaged = "Space drive damaged - Maximum speed is 0.2";
    public const string NAV_Invalid_DIR = "Invalid NAV Direction: ";
    public const string NAV_Invalid_DIST = "Invalid NAV Distance: ";
    public const string NAV_ImpulseEngaged = "Impulse Engaged";
    public const string NAV_SpaceDriveEngaged = "Space drive engaged";
    public const string NAV_ImpulseEnginesShutDown = "Impulse Engines Shut Down At Sector ";
    public const string NAV_BadNavigation = " Due To Bad Navigation";
    public const string NAV_NotEnoughEnergy_ShieldEnergyAvail = "Insufficient energy to make the trip, but there is shield energy available.";

    public const string MAINT_Docked = "The Starship has docked with the starbase.";
    public const string MAINT_TechWaiting = "Technicians are waiting to repair damages.";
    public const string MAINT_ItWillTake = "It will take ";
    public const string MAINT_MissionTimeUnits = " mission time units.";
    public const string MAINT_ReplyYES = "Reply 'YES' to make the repairs.";
    public const string MAINT_RepairsMade = "Repairs complete.";
    public const string MAINT_RepairsComplete = "Repairs are complete on ";
    public const string MAINT_EnemyFiredShieldHit = "Enemy Fired! {0} hit from enemy in sector[{1}].  Shield level now {2}.";
    public const string MAINT_EnemyFiredProtected = "Enemy Fired! {0} hit from enemy in sector[{1}].  ";
    public const string MAINT_StarBaseShieldsProtected = "StarBase shields are protecting the Starship.";
    public const string MAINT_EnemyFiredStarshipDestroyed = "Enemy Fired! {0} hit from enemy in sector[{1}].  Starship was destroyed!!!";
    public const string MAINT_DeviceDamaged = " has been damaged. ";
    public const string MAINT_MissionTimeUnitsToRepair = " mission time units to repair.";
    public const string MAINT_NewQuadrantEnemiesRedAlert = "Enemies in this quadrant, red alert!";
    public const string MAINT_EnemyMoved = "Enemy Moved! Enemy at sector [{0}] moved to sector [{1}].";

    public const string END_MissionFailed_StarshipDestroyed = "Mission Failed: Starship was destroyed! The frontier will be overrun.";
    public const string END_MissionFailed_RanOutOfEnergy = "Mission Failed: Starship ran out of energy and is left stranded. The frontier will be overrun.";
    public const string END_MissionFailed_RanOutOfTime = "Mission Failed: You ran out of time. The frontier will be overrun.";
    public const string END_MissionFailed_Quit = "Mission Failed: You abandoned your post. The frontier will be overrun.";
    public const string END_MissionAccomplished = "Mission Accomplished!  All Enemy ships were destroyed!";
    public const string END_EnergyGoneShieldEnergyAvail = "Insufficient Energy, but can be transferred from shield control";
}