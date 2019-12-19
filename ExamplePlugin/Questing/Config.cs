using BepInEx.Configuration;
using RoR2;
using System;
using System.Globalization;

namespace RPGMod {
namespace Questing {


// The variables that are accessable by the config.
public class Config
{
    // Chance params
    public static float dropChanceNormalEnemy;
    public static float dropChanceEliteEnemy;
    public static float dropChanceBossEnemy;
    public static float questChanceCommon;
    public static float questChanceUncommon;
    public static float questChanceLegendary;
    public static float eliteChanceCommon;
    public static float eliteChanceUncommon;
    public static float eliteChanceLegendary;
    public static float eliteChanceLunar;
    public static float normalChanceCommon;
    public static float normalChanceUncommon;
    public static float normalChanceLegendary;
    public static float normalChanceEquip;
    public static float earlyChanceScaling;
    public static float playerChanceScaling;

    // UI params
    public static int screenPosX;
    public static int screenPosY;
    public static int sizeX;
    public static int sizeY;

    // Questing params
    public static int questObjectiveMin;
    public static int questObjectiveMax;
    public static int questAmountMax;
    public static bool dropItemsFromPlayers;
    public static bool displayQuestsInChat;
    public static float questCooldown;
    public static int questCooldownTime;

    // Feature params
    public static bool defaultWorldSpawnsEnabled;
    public static bool enemyItemDropsEnabled;
    public static bool questingEnabled;
    public static bool restartQuestsOnStageChange;

    // Misc params
    public static string[] bannedDirectorSpawns;
    public static float worldSpawnPercentage = 1.0f;
    public static short questPort;

    // Converts string config to a float
    private static float ToFloat(string configline)
    {
        if (float.TryParse(configline, NumberStyles.Any, CultureInfo.InvariantCulture, out float x))
        {
            return x;
        }
        return 0f;
    }

    // Refreshes the config values from the config
    public static void Load(ConfigFile config, bool reload)
    {
        if (reload)
        {
            config.Reload();
        }

        // Chances
        dropChanceNormalEnemy = config.Bind<float>(new ConfigDefinition("Chances","dropChanceNormalEnemy"), 1.5f, new ConfigDescription("Item drop chance for a normal enemy")).Value;
        dropChanceEliteEnemy = ToFloat(config.Wrap("Chance", "dropChanceEliteEnemy", "Item drop chance for an elite enemy", "2.5").Value);
        dropChanceBossEnemy = ToFloat(config.Wrap("Chance", "dropChanceBossEnemy", "Item drop chance for a boss", "10.0").Value);
        questChanceCommon = ToFloat(config.Wrap("Chance", "questChanceCommon", "Quest reward chance for a common item", "0").Value);
        questChanceUncommon = ToFloat(config.Wrap("Chance", "questChanceUncommon", "Quest reward chance for a uncommon item", "0.92").Value);
        questChanceLegendary = ToFloat(config.Wrap("Chance", "questChanceLegendary", "Quest reward chance for a legendary item", "0.08").Value);
        eliteChanceCommon = ToFloat(config.Wrap("Chance", "eliteChanceCommon", "Elite enemy common item drop chance", "0.45").Value);
        eliteChanceUncommon = ToFloat(config.Wrap("Chance", "eliteChanceUncommon", "Elite enemy uncommon item drop chance", "0.2").Value);
        eliteChanceLegendary = ToFloat(config.Wrap("Chance", "eliteChanceLegendary", "Elite enemy legendary item drop chance", "0.1").Value);
        eliteChanceLunar = ToFloat(config.Wrap("Chance", "eliteChanceLunar", "Elite enemy lunar item drop chance", "0.1").Value);
        normalChanceCommon = ToFloat(config.Wrap("Chance", "normalChanceCommon", "Normal enemy common item drop chance", "0.9").Value);
        normalChanceUncommon = ToFloat(config.Wrap("Chance", "normalChanceUncommon", "Normal enemy uncommon item drop chance", "0.1").Value);
        normalChanceLegendary = ToFloat(config.Wrap("Chance", "normalChanceLegendary", "Normal enemy legendary item drop chance", "0.01").Value);
        normalChanceEquip = ToFloat(config.Wrap("Chance", "normalChanceEquip", "Normal enemy equipment item drop chance", "0.1").Value);
        playerChanceScaling = ToFloat(config.Wrap("Chance", "playerChanceScaling", "The percentage chance overall increase per player (helps with player scaling)", "0.35").Value);
        earlyChanceScaling = ToFloat(config.Wrap("Chance", "earlyChanceScaling", "Percentage chance increase for the early stage of the game", "1.5").Value);

        // UI params
        screenPosX = config.Wrap("UI", "screenPosX", "UI location on the x axis (percentage of screen width)", 89).Value;
        screenPosY = config.Wrap("UI", "screenPosY", "UI location on the y axis (percentage of screen height)", 50).Value;
        sizeX = config.Wrap("UI", "sizeX", "Size of UI on the x axis (pixels)", 300).Value;
        sizeY = config.Wrap("UI", "sizeY", "Size of UI on the x axis (pixels)", 100).Value;

        // Questing params
        questObjectiveMin = config.Wrap("Questing", "questObjectiveMin", "Minimum quest objective", 5).Value; // Needs changing for kills
        questObjectiveMax = config.Wrap("Questing", "questObjectiveMax", "Maximum quest objective", 20).Value; // Needs changing for kills
        questAmountMax = config.Wrap("Questing", "questAmountMax", "The maximum amount of quests", 3).Value;
        dropItemsFromPlayers = Convert.ToBoolean(config.Wrap("Questing", "dropItemsFromPlayers", "Items drop from player instead of popping up in inventory", "false").Value);
        displayQuestsInChat = Convert.ToBoolean(config.Wrap("Questing", "displayQuestInChat", "Quests show up in chat (useful when playing with unmodded players)", "true").Value);
        questCooldownTime = config.Wrap("Questing", "questCooldown", "The cooldown time for a quest to appear (seconds)", 60).Value;
        questCooldown = 0 - questCooldownTime;

        // Director params
        worldSpawnPercentage = ToFloat(config.Wrap("Director", "worldSpawnPercentage", "World spawn percentage for the director", "1.0").Value);
        bannedDirectorSpawns = config.Wrap("Director", "bannedDirectorSpawns", "A comma seperated list of banned spawns for the director", "Chest,TripleShop,Chance,Equipment,Blood").Value.Split(',');
        defaultWorldSpawnsEnabled = Convert.ToBoolean(config.Wrap("Director", "defaultWorldSpawnsEnabled", "Whether or not to use default world spawns or banned spawns list", "true").Value);

        // Feature params
        questingEnabled = Convert.ToBoolean(config.Wrap("Features", "questingEnabled", "Quests enabled", "true").Value);
        enemyItemDropsEnabled = Convert.ToBoolean(config.Wrap("Features", "enemyItemDropsEnabled", "Enemies drop items", "true").Value);
        restartQuestsOnStageChange = Convert.ToBoolean(config.Wrap("Features", "restartQuestsOnStageChange", "Quests reset on stage change", "false").Value);

        questPort = config.Bind<short>(new ConfigDefinition("Networking","questPort"), 1337, new ConfigDescription("The port used for the quest networking")).Value;

        Chat.AddMessage("<color=#13d3dd>RPGMod: </color> config loaded");
    }
}


}
}