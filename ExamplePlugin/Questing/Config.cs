using BepInEx.Configuration;
using RoR2;

namespace RPGMod.Questing
{
    // All the config variables
    public class Config
    {
        // Attributes

        // Chance params
        public static float NormalDropChance;
        public static float EliteDropChance;
        public static float BossDropChance;
        public static float QuestChanceCommon;
        public static float QuestChanceUncommon;
        public static float QuestChanceLegendary;
        public static float EliteChanceCommon;
        public static float EliteChanceUncommon;
        public static float EliteChanceLegendary;
        public static float EliteChanceLunar;
        public static float NormalChanceCommon;
        public static float NormalChanceUncommon;
        public static float NormalChanceLegendary;
        public static float NormalChanceEquip;
        public static float EarlyChanceScaling;
        public static float PlayerChanceScaling;

        // UI params
        public static float XPositionUI;
        public static float YPositionUI;
        public static bool UseGameHudScale;

        // Questing params
        public static int QuestKillObjectiveMin;
        public static int QuestKillObjectiveMax;
        public static int QuestUtilityObjectiveMin;
        public static int QuestUtilityObjectiveMax;
        public static float QuestObjectiveCommonMultiplier;
        public static float QuestObjectiveUncommonMultiplier;
        public static float QuestObjectiveLegendaryMultiplier;
        public static int QuestPerTypeMax;
        public static int QuestAmountMax;
        public static bool DropItemsFromPlayers;
        public static bool DisplayQuestsInChat;
        public static int QuestCooldownTimeOnSceneChange;
        public static int QuestCooldownTime;

        // Feature params
        public static bool DefaultWorldSpawnsEnabled;
        public static bool EnemyItemDropsEnabled;
        public static bool QuestingEnabled;
        public static bool RestartQuestsOnStageChange;

        // Misc params
        public static bool DropItemsFromEnemies;
        public static string[] BannedDirectorSpawns;
        public static float WorldSpawnPercentage = 1.0f;
        public static short QuestPort;

        // Refreshes the config values from the config
        public static void Load(ConfigFile config, bool reload)
        {
            if (reload)
            {
                config.Reload();
            }

            // Chances
            NormalDropChance = config.Bind(new ConfigDefinition("Chances", "normalDropChance"), 1.5f, new ConfigDescription("Item drop chance for a normal enemy")).Value;
            EliteDropChance = config.Bind(new ConfigDefinition("Chances", "eliteDropChance"), 2.5f, new ConfigDescription("Item drop chance for an elite enemy")).Value;
            BossDropChance = config.Bind(new ConfigDefinition("Chances", "bossDropChance"), 10.0f, new ConfigDescription("Item drop chance for a boss")).Value;
            QuestChanceCommon = config.Bind(new ConfigDefinition("Chances", "questChanceCommon"), 0.0f, new ConfigDescription("Chance for a quest reward to be a common item")).Value / 100;
            QuestChanceUncommon = config.Bind(new ConfigDefinition("Chances", "questChanceUncommon"), 97.0f, new ConfigDescription("Chance for a quest reward to be a uncommon item")).Value / 100;
            QuestChanceLegendary = config.Bind(new ConfigDefinition("Chances", "questChanceLegendary"), 3.0f, new ConfigDescription("Chance for a quest reward to be a legendary item")).Value / 100;
            EliteChanceCommon = config.Bind(new ConfigDefinition("Chances", "eliteChanceCommon"), 45.0f, new ConfigDescription("Elite enemy common item drop chance")).Value / 100;
            EliteChanceUncommon = config.Bind(new ConfigDefinition("Chances", "eliteChanceUncommon"), 20.0f, new ConfigDescription("Elite enemy uncommon item drop chance")).Value / 100;
            EliteChanceLegendary = config.Bind(new ConfigDefinition("Chances", "eliteChanceLegendary"), 8.0f, new ConfigDescription("Elite enemy legendary item drop chance")).Value / 100;
            EliteChanceLunar = config.Bind(new ConfigDefinition("Chances", "eliteChanceLunar"), 10.0f, new ConfigDescription("Elite enemy lunar item drop chance")).Value / 100;
            NormalChanceCommon = config.Bind(new ConfigDefinition("Chances", "normalChanceCommon"), 90.0f, new ConfigDescription("Normal enemy common item drop chance")).Value / 100;
            NormalChanceUncommon = config.Bind(new ConfigDefinition("Chances", "normalChanceUncommon"), 10.0f, new ConfigDescription("Normal enemy uncommon item drop chance")).Value / 100;
            NormalChanceLegendary = config.Bind(new ConfigDefinition("Chances", "normalChanceLegendary"), 0.0f, new ConfigDescription("Normal enemy legendary item drop chance")).Value / 100;
            NormalChanceEquip = config.Bind(new ConfigDefinition("Chances", "normalChanceEquip"), 10.0f, new ConfigDescription("Normal enemy equipment item drop chance")).Value / 100;
            PlayerChanceScaling = config.Bind(new ConfigDefinition("Chances", "playerChanceScaling"), 35.0f, new ConfigDescription("The percentage chance overall increase per player (helps with player scaling)")).Value / 100;
            EarlyChanceScaling = config.Bind(new ConfigDefinition("Chances", "earlyChanceScaling"), 150.0f, new ConfigDescription("Percentage chance increase for the early stage of the game")).Value / 100;
            DropItemsFromEnemies = config.Bind(new ConfigDefinition("Chances", "dropItemsFromEnemies"), true, new ConfigDescription("Items drop from enemies instead of popping up in inventory")).Value;

            // UI params
            XPositionUI = (config.Bind(new ConfigDefinition("UI", "xPositionUI"), 89.5f, new ConfigDescription("UI location on the x axis (percentage of screen width)")).Value - 50) / 100;
            YPositionUI = (config.Bind(new ConfigDefinition("UI", "yPositionUI"), 50.0f, new ConfigDescription("UI location on the y axis (percentage of screen height)")).Value - 50) / 100;
            UseGameHudScale = config.Bind(new ConfigDefinition("UI", "useGameHudScale"), true, new ConfigDescription("Whether or not to use the games inbuilt hud scale option for questing UI")).Value;

            // Questing params
            QuestKillObjectiveMin = config.Bind(new ConfigDefinition("Questing", "questKillObjectiveMin"), 5, new ConfigDescription("Minimum kill quest objective (Gets scaled over time to the max value)")).Value;
            QuestKillObjectiveMax = config.Bind(new ConfigDefinition("Questing", "questKillObjectiveMax"), 15, new ConfigDescription("Maximum kill quest objective (Gets scaled over time from the min value)")).Value;
            QuestUtilityObjectiveMin = config.Bind(new ConfigDefinition("Questing", "questUtilityObjectiveMin"), 4, new ConfigDescription("Minimum utility (chest opening, etc) quest objective (Gets scaled over time to the max value)")).Value;
            QuestUtilityObjectiveMax = config.Bind(new ConfigDefinition("Questing", "questUtilityObjectiveMax"), 9, new ConfigDescription("Maximum utility (chest opening, etc) quest objective (Gets scaled over time from the min value)")).Value;
            QuestObjectiveCommonMultiplier = config.Bind(new ConfigDefinition("Questing", "questObjectiveCommonMultiplier"), 85f, new ConfigDescription("Quest objective scaling for a common reward")).Value / 100;
            QuestObjectiveUncommonMultiplier = config.Bind(new ConfigDefinition("Questing", "questObjectiveUncommonMultiplier"), 100f, new ConfigDescription("Quest objective scaling for an uncommon reward")).Value / 100;
            QuestObjectiveLegendaryMultiplier = config.Bind(new ConfigDefinition("Questing", "questObjectiveLegendaryMultiplier"), 200f, new ConfigDescription("Quest objective scaling for a legendary reward")).Value / 100;
            QuestPerTypeMax = config.Bind(new ConfigDefinition("Questing", "questPerTypeMax"), 1, new ConfigDescription("The maximum amount of quests of each questType")).Value;
            QuestAmountMax = config.Bind(new ConfigDefinition("Questing", "questAmountMax"), 3, new ConfigDescription("The maximum amount of quests")).Value;
            DropItemsFromPlayers = config.Bind(new ConfigDefinition("Questing", "dropItemsFromPlayers"), false, new ConfigDescription("Items drop from player instead of popping up in inventory")).Value;
            DisplayQuestsInChat = config.Bind(new ConfigDefinition("Questing", "displayQuestInChat"), true, new ConfigDescription("Quests show up in chat (useful when playing with unmodded players)")).Value;
            QuestCooldownTimeOnSceneChange = config.Bind(new ConfigDefinition("Questing", "questCooldownTimeOnSceneChange"), 15, new ConfigDescription("The cooldown time for a quest to appear after the scene has changed (seconds)")).Value;
            QuestCooldownTime = config.Bind(new ConfigDefinition("Questing", "questCooldown"), 65, new ConfigDescription("The cooldown time for a quest to appear (seconds)")).Value;

            // Director params
            WorldSpawnPercentage = config.Bind(new ConfigDefinition("Director", "worldSpawnPercentage"), 100.0f, new ConfigDescription("World spawn percentage for the director")).Value / 100;
            BannedDirectorSpawns = config.Bind(new ConfigDefinition("Director", "bannedDirectorSpawns"), "Chest,TripleShop,Chance,Equipment,Blood", new ConfigDescription("A comma seperated list of banned spawns for the director")).Value.Split(',');
            DefaultWorldSpawnsEnabled = config.Bind(new ConfigDefinition("Director", "defaultWorldSpawnsEnabled"), true, new ConfigDescription("Whether or not to use default world spawns or banned spawns list")).Value;

            // Feature params
            QuestingEnabled = config.Bind(new ConfigDefinition("Features", "questingEnabled"), true, new ConfigDescription("Quests enabled")).Value;
            EnemyItemDropsEnabled = config.Bind(new ConfigDefinition("Features", "enemyItemDropsEnabled"), true, new ConfigDescription("Enemies drop items on death")).Value;
            RestartQuestsOnStageChange = config.Bind(new ConfigDefinition("Features", "restartQuestsOnStageChange"), false, new ConfigDescription("Quests reset on stage change")).Value;

            QuestPort = config.Bind<short>(new ConfigDefinition("Networking", "questPort"), 1337, new ConfigDescription("The port used for the quest networking")).Value;

            Chat.AddMessage("<color=#13d3dd>RPGMod: </color> Config loaded");
        }
    }
}