using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace RPGMod
{
    public static class ModConfig
    {
        // Chance params
        public static float chanceNormal;
        public static float chanceElite;
        public static float chanceBoss;
        public static float bossChestChanceLegendary;
        public static float bossChestChanceUncommon;
        public static float chanceQuestingCommon;
        public static float chanceQuestingUnCommon;
        public static float chanceQuestingLegendary;
        public static float dropsPlayerScaling;
        public static float eliteChanceTier1;
        public static float eliteChanceTier2;
        public static float eliteChanceTier3;
        public static float eliteChanceTierLunar;
        public static float normalChanceTier1;
        public static float normalChanceTier2;
        public static float normalChanceTier3;
        public static float normalChanceTierEquip;
        public static float gameStartScaling;

        // UI params
        public static int screenPosX;
        public static int screenPosY;
        //public int titleFontSize;
        //public int descriptionFontSize;
        public static int sizeX;
        public static int sizeY;

        // Questing params
        public static int questObjectiveFactor;
        public static int questObjectiveLimit;
        public static bool itemDroppingFromPlayers;
        public static bool questInChat;
        public static int questIndex;
        public static int questAmount;

        // Feature params
        public static bool isChests;
        public static bool isBossChests;
        public static bool isEnemyDrops;
        public static bool isQuesting;
        public static bool isQuestResetting;
    
        // Misc params
        public static String[] bannedDirectorSpawns;
        public static float percentSpawns = 1.0f;

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
        public static void RefreshValues(ConfigFile Config, bool initLoad)
        {
            if (!initLoad)
            {
                Config.Reload();
            }

            // Chances
            chanceNormal = ToFloat(Config.Wrap("Chances", "chanceNormal", "Base chance for a normal enemy to drop an item (float)", "9.5").Value);
            chanceElite = ToFloat(Config.Wrap("Chances", "chanceElite", "Base chance for an elite enemy to drop an item (float)", "11.0").Value);
            chanceBoss = ToFloat(Config.Wrap("Chances", "chanceBoss", "Base chance for a boss enemy to drop an item (float)", "35.0").Value);
            bossChestChanceLegendary = ToFloat(Config.Wrap("Chances", "bossChestChanceLegendary", "Chance for a legendary to drop from a boss chest (float)", "0.3").Value);
            bossChestChanceUncommon = ToFloat(Config.Wrap("Chances", "bossChestChanceUncommon", "Chance for a uncommon to drop from a boss chest (float)", "0.7").Value);
            chanceQuestingCommon = ToFloat(Config.Wrap("Chances", "chanceQuestingCommon", "Chance for quest drop to be common (float)", "0").Value);
            chanceQuestingUnCommon = ToFloat(Config.Wrap("Chances", "chanceQuestingUnCommon", "Chance for quest drop to be uncommon (float)", "0.92").Value);
            chanceQuestingLegendary = ToFloat(Config.Wrap("Chances", "chanceQuestingLegendary", "Chance for quest drop to be legendary (float)", "0.08").Value);
            dropsPlayerScaling = ToFloat(Config.Wrap("Chances", "dropsPlayerScaling", "Scaling per player (drop chance percentage increase per player) (float)", "0.35").Value);
            eliteChanceTier1 = ToFloat(Config.Wrap("Chances", "eliteChanceTier1", "Chance for elite to drop a tier 1 item (float)", "0.45").Value);
            eliteChanceTier2 = ToFloat(Config.Wrap("Chances", "eliteChanceTier2", "Chance for elite to drop a tier 2 item (float)", "0.2").Value);
            eliteChanceTier3 = ToFloat(Config.Wrap("Chances", "eliteChanceTier3", "Chance for elite to drop a tier 3 item (float)", "0.1").Value);
            eliteChanceTierLunar = ToFloat(Config.Wrap("Chances", "eliteChanceTierLunar", "Chance for elite to drop a lunar item (float)", "0.1").Value);
            normalChanceTier1 = ToFloat(Config.Wrap("Chances", "normalChanceTier1", "Chance for normal enemy to drop a tier 1 item (float)", "0.9").Value);
            normalChanceTier2 = ToFloat(Config.Wrap("Chances", "normalChanceTier2", "Chance for normal enemy to drop a tier 2 item (float)", "0.1").Value);
            normalChanceTier3 = ToFloat(Config.Wrap("Chances", "normalChanceTier3", "Chance for normal enemy to drop a tier 3 item (float)", "0.01").Value);
            normalChanceTierEquip = ToFloat(Config.Wrap("Chances", "normalChanceTierEquip", "Chance for normal enemy to drop equipment (float)", "0.1").Value);
            gameStartScaling = ToFloat(Config.Wrap("Chances", "gameStartScaling", "Scaling of chances for the start of the game, that goes away during later stages (float)", "1.5").Value);

            // UI params
            screenPosX = Config.Wrap("UI", "Screen Pos X", "UI location on the x axis (percentage of screen width) (int)", 89).Value;
            screenPosY = Config.Wrap("UI", "Screen Pos Y", "UI location on the y axis (percentage of screen height) (int)", 50).Value;
            sizeX = Config.Wrap("UI", "Size X", "Size of UI on the x axis (pixels)", 300).Value;
            sizeY = Config.Wrap("UI", "Size Y", "Size of UI on the x axis (pixels) (int)", 80).Value;

            // Questing params
            questObjectiveFactor = Config.Wrap("Questing", "Quest Objective Minimum", "The factor for quest objective values (int)", 8).Value;
            questObjectiveLimit = Config.Wrap("Questing", "Quest Objective Limit", "The factor for the max quest objective value (int)", 20).Value;
            questAmount = Config.Wrap("Questing", "Amount of quests", "The maximum amount of quests (int)", 3).Value;
            itemDroppingFromPlayers = Convert.ToBoolean(Config.Wrap("Questing", "itemDroppingFromPlayers", "Items drop from player instead of popping up in inventory (bool)", "false").Value);
            questInChat = Convert.ToBoolean(Config.Wrap("Questing", "questInChat", "Quests show up in chat (useful when playing with unmodded players) (bool)", "true").Value);

            // Director params
            percentSpawns = ToFloat(Config.Wrap("Director", "percentSpawns", "Percentage amount of world spawns", "1.0").Value);
            bannedDirectorSpawns = Config.Wrap("Director", "bannedDirectorSpawns", "A comma seperated list of banned spawns for director", "Chest,TripleShop,Chance,Equipment,Blood").Value.Split(',');
            isChests = Convert.ToBoolean(Config.Wrap("Director", "Interactables", "Use banned director spawns (bool)", "true").Value);

            // Feature params
            isQuesting = Convert.ToBoolean(Config.Wrap("Features", "Questing", "Questing system (bool)", "true").Value);
            isEnemyDrops = Convert.ToBoolean(Config.Wrap("Features", "Enemy Drops", "Enemies drop items (bool)", "true").Value);
            isQuestResetting = Convert.ToBoolean(Config.Wrap("Features", "Quest Resetting", "Determines whether quests reset over stage advancement (bool)", "false").Value);

            // force UI refresh and send message
            Chat.AddMessage("<color=#13d3dd>RPGMod: </color> Config loaded");
        }
    }
}
