using BepInEx.Configuration;
using RoR2;
using System;
using System.Globalization;

namespace RPGMod
{
    public static class ModConfig
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

        // Feature params
        public static bool defaultWorldSpawnsEnabled;
        public static bool enemyItemDropsEnabled;
        public static bool questingEnabled;
        public static bool restartQuestsOnStageChange;
    
        // Misc params
        public static string[] bannedDirectorSpawns;
        public static float worldSpawnPercentage = 1.0f;

        // Converts string config to a float
        public static float ToFloat(string configline)
        {
            if (float.TryParse(configline, NumberStyles.Any, CultureInfo.InvariantCulture, out float x))
            {
                return x;
            }
            return 0f;
        }

        // Refreshes the config values from the config
        public static void ReloadConfig(ConfigFile Config, bool initLoad)
        {
            if (!initLoad)
            {
                Config.Reload();
            }

            // Chances
            dropChanceNormalEnemy = ToFloat(Config.Wrap("Chance", "dropChanceNormalEnemy", "Item drop chance for a normal enemy", "9.5").Value);
            dropChanceEliteEnemy = ToFloat(Config.Wrap("Chance", "dropChanceEliteEnemy", "Item drop chance for an elite enemy", "11.0").Value);
            dropChanceBossEnemy = ToFloat(Config.Wrap("Chance", "dropChanceBossEnemy", "Item drop chance for a boss", "35.0").Value);
            questChanceCommon = ToFloat(Config.Wrap("Chance", "questChanceCommon", "Quest reward chance for a common item", "0").Value);
            questChanceUncommon = ToFloat(Config.Wrap("Chance", "questChanceUncommon", "Quest reward chance for a uncommon item", "0.92").Value);
            questChanceLegendary = ToFloat(Config.Wrap("Chance", "questChanceLegendary", "Quest reward chance for a legendary item", "0.08").Value);
            eliteChanceCommon = ToFloat(Config.Wrap("Chance", "eliteChanceCommon", "Elite enemy common item drop chance", "0.45").Value);
            eliteChanceUncommon = ToFloat(Config.Wrap("Chance", "eliteChanceUncommon", "Elite enemy uncommon item drop chance", "0.2").Value);
            eliteChanceLegendary = ToFloat(Config.Wrap("Chance", "eliteChanceLegendary", "Elite enemy legendary item drop chance", "0.1").Value);
            eliteChanceLunar = ToFloat(Config.Wrap("Chance", "eliteChanceLunar", "Elite enemy lunar item drop chance", "0.1").Value);
            normalChanceCommon = ToFloat(Config.Wrap("Chance", "normalChanceCommon", "Normal enemy common item drop chance", "0.9").Value);
            normalChanceUncommon = ToFloat(Config.Wrap("Chance", "normalChanceUncommon", "Normal enemy uncommon item drop chance", "0.1").Value);
            normalChanceLegendary = ToFloat(Config.Wrap("Chance", "normalChanceLegendary", "Normal enemy legendary item drop chance", "0.01").Value);
            normalChanceEquip = ToFloat(Config.Wrap("Chance", "normalChanceEquip", "Normal enemy equipment item drop chance", "0.1").Value);
            playerChanceScaling = ToFloat(Config.Wrap("Chance", "playerChanceScaling", "The percentage chance overall increase per player (helps with player scaling)", "0.35").Value);
            earlyChanceScaling = ToFloat(Config.Wrap("Chance", "earlyChanceScaling", "Percentage chance increase for the early stage of the game", "1.5").Value);

            // UI params
            screenPosX = Config.Wrap("UI", "screenPosX", "UI location on the x axis (percentage of screen width)", 89).Value;
            screenPosY = Config.Wrap("UI", "screenPosY", "UI location on the y axis (percentage of screen height)", 50).Value;
            sizeX = Config.Wrap("UI", "sizeX", "Size of UI on the x axis (pixels)", 300).Value;
            sizeY = Config.Wrap("UI", "sizeY", "Size of UI on the x axis (pixels)", 100).Value;

            // Questing params
            questObjectiveMin = Config.Wrap("Questing", "questObjectiveMin", "Minimum quest objective", 5).Value; // Needs changing for kills
            questObjectiveMax = Config.Wrap("Questing", "questObjectiveMax", "Maximum quest objective", 20).Value; // Needs changing for kills
            questAmountMax = Config.Wrap("Questing", "questAmountMax", "The maximum amount of quests", 3).Value;
            dropItemsFromPlayers = Convert.ToBoolean(Config.Wrap("Questing", "dropItemsFromPlayers", "Items drop from player instead of popping up in inventory", "false").Value);
            displayQuestsInChat = Convert.ToBoolean(Config.Wrap("Questing", "displayQuestInChat", "Quests show up in chat (useful when playing with unmodded players)", "true").Value);

            // Director params
            worldSpawnPercentage = ToFloat(Config.Wrap("Director", "worldSpawnPercentage", "World spawn percentage for the director", "1.0").Value);
            bannedDirectorSpawns = Config.Wrap("Director", "bannedDirectorSpawns", "A comma seperated list of banned spawns for the director", "Chest,TripleShop,Chance,Equipment,Blood").Value.Split(',');
            defaultWorldSpawnsEnabled = Convert.ToBoolean(Config.Wrap("Director", "defaultWorldSpawnsEnabled", "Whether or not to use default world spawns or banned spawns list", "true").Value);

            // Feature params
            questingEnabled = Convert.ToBoolean(Config.Wrap("Features", "questingEnabled", "Quests enabled", "true").Value);
            enemyItemDropsEnabled = Convert.ToBoolean(Config.Wrap("Features", "enemyItemDropsEnabled", "Enemies drop items", "true").Value);
            restartQuestsOnStageChange = Convert.ToBoolean(Config.Wrap("Features", "restartQuestsOnStageChange", "Quests reset on stage change", "false").Value);

            // Force UI refresh and send message
            MainDefs.resetUI = true;
            Chat.AddMessage("<color=#13d3dd>RPGMod: </color> Config loaded");
        }
    }
}
