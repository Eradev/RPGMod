using BepInEx.Configuration;
using RoR2;

namespace RPGMod
{
    namespace Questing
    {
        // All the config variables
        public class Config
        {
            // Attributes

            // Chance params
            public static float normalDropChance;
            public static float eliteDropChance;
            public static float bossDropChance;
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
            public static float xPositionUI;
            public static float yPositionUI;
            public static bool useGameHudScale;

            // Questing params
            public static int questKillObjectiveMin;
            public static int questKillObjectiveMax;
            public static int questUtilityObjectiveMin;
            public static int questUtilityObjectiveMax;
            public static int questPerTypeMax;
            public static int questAmountMax;
            public static bool dropItemsFromPlayers;
            public static bool displayQuestsInChat;
            public static int questCooldownTimeOnSceneChange;
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

            // Methods

            // Refreshes the config values from the config
            public static void Load(ConfigFile config, bool reload)
            {
                if (reload)
                {
                    config.Reload();
                }

                // Chances
                normalDropChance = config.Bind<float>(new ConfigDefinition("Chances", "normalDropChance"), 1.5f, new ConfigDescription("Item drop chance for a normal enemy")).Value;
                eliteDropChance = config.Bind<float>(new ConfigDefinition("Chances", "eliteDropChance"), 2.5f, new ConfigDescription("Item drop chance for an elite enemy")).Value;
                bossDropChance = config.Bind<float>(new ConfigDefinition("Chances", "bossDropChance"), 10.0f, new ConfigDescription("Item drop chance for a boss")).Value;
                questChanceCommon = config.Bind<float>(new ConfigDefinition("Chances", "questChanceCommon"), 0.0f, new ConfigDescription("Chance for a quest reward to be a common item")).Value / 100;
                questChanceUncommon = config.Bind<float>(new ConfigDefinition("Chances", "questChanceUncommon"), 97.0f, new ConfigDescription("Chance for a quest reward to be a uncommon item")).Value / 100;
                questChanceLegendary = config.Bind<float>(new ConfigDefinition("Chances", "questChanceLegendary"), 3.0f, new ConfigDescription("Chance for a quest reward to be a legendary item")).Value / 100;
                eliteChanceCommon = config.Bind<float>(new ConfigDefinition("Chances", "eliteChanceCommon"), 45.0f, new ConfigDescription("Elite enemy common item drop chance")).Value / 100;
                eliteChanceUncommon = config.Bind<float>(new ConfigDefinition("Chances", "eliteChanceUncommon"), 20.0f, new ConfigDescription("Elite enemy uncommon item drop chance")).Value / 100;
                eliteChanceLegendary = config.Bind<float>(new ConfigDefinition("Chances", "eliteChanceLegendary"), 8.0f, new ConfigDescription("Elite enemy legendary item drop chance")).Value / 100;
                eliteChanceLunar = config.Bind<float>(new ConfigDefinition("Chances", "eliteChanceLunar"), 10.0f, new ConfigDescription("Elite enemy lunar item drop chance")).Value / 100;
                normalChanceCommon = config.Bind<float>(new ConfigDefinition("Chances", "normalChanceCommon"), 90.0f, new ConfigDescription("Normal enemy common item drop chance")).Value / 100;
                normalChanceUncommon = config.Bind<float>(new ConfigDefinition("Chances", "normalChanceUncommon"), 10.0f, new ConfigDescription("Normal enemy uncommon item drop chance")).Value / 100;
                normalChanceLegendary = config.Bind<float>(new ConfigDefinition("Chances", "normalChanceLegendary"), 0.0f, new ConfigDescription("Normal enemy legendary item drop chance")).Value / 100;
                normalChanceEquip = config.Bind<float>(new ConfigDefinition("Chances", "normalChanceEquip"), 10.0f, new ConfigDescription("Normal enemy equipment item drop chance")).Value / 100;
                playerChanceScaling = config.Bind<float>(new ConfigDefinition("Chances", "playerChanceScaling"), 35.0f, new ConfigDescription("The percentage chance overall increase per player (helps with player scaling)")).Value / 100;
                earlyChanceScaling = config.Bind<float>(new ConfigDefinition("Chances", "earlyChanceScaling"), 150.0f, new ConfigDescription("Percentage chance increase for the early stage of the game")).Value / 100;

                // UI params
                xPositionUI = (config.Bind<float>(new ConfigDefinition("UI", "xPositionUI"), 89.5f, new ConfigDescription("UI location on the x axis (percentage of screen width)")).Value - 50) / 100;
                yPositionUI = (config.Bind<float>(new ConfigDefinition("UI", "yPositionUI"), 50.0f, new ConfigDescription("UI location on the y axis (percentage of screen height)")).Value - 50) / 100;
                useGameHudScale = config.Bind<bool>(new ConfigDefinition("UI", "useGameHudScale"), true, new ConfigDescription("Whether or not to use the games inbuilt hud scale option for questing UI")).Value;

                // Questing params
                questKillObjectiveMin = config.Bind<int>(new ConfigDefinition("Questing", "questKillObjectiveMin"), 5, new ConfigDescription("Minimum kill quest objective (Gets scaled over time to the max value)")).Value;
                questKillObjectiveMax = config.Bind<int>(new ConfigDefinition("Questing", "questKillObjectiveMax"), 15, new ConfigDescription("Maximum kill quest objective (Gets scaled over time from the min value)")).Value;
                questUtilityObjectiveMin = config.Bind<int>(new ConfigDefinition("Questing", "questUtilityObjectiveMin"), 4, new ConfigDescription("Minimum utility (chest opening, etc) quest objective (Gets scaled over time to the max value)")).Value;
                questUtilityObjectiveMax = config.Bind<int>(new ConfigDefinition("Questing", "questUtilityObjectiveMax"), 9, new ConfigDescription("Maximum utility (chest opening, etc) quest objective (Gets scaled over time from the min value)")).Value;
                questPerTypeMax = config.Bind<int>(new ConfigDefinition("Questing", "questPerTypeMax"), 1, new ConfigDescription("The maximum amount of quests of each type")).Value;
                questAmountMax = config.Bind<int>(new ConfigDefinition("Questing", "questAmountMax"), 3, new ConfigDescription("The maximum amount of quests")).Value;
                dropItemsFromPlayers = config.Bind<bool>(new ConfigDefinition("Questing", "dropItemsFromPlayers"), false, new ConfigDescription("Items drop from player instead of popping up in inventory")).Value;
                displayQuestsInChat = config.Bind<bool>(new ConfigDefinition("Questing", "displayQuestInChat"), true, new ConfigDescription("Quests show up in chat (useful when playing with unmodded players)")).Value;
                questCooldownTimeOnSceneChange = config.Bind<int>(new ConfigDefinition("Questing", "questCooldownTimeOnSceneChange"), 15, new ConfigDescription("The cooldown time for a quest to appear after the scene has changed (seconds)")).Value;
                questCooldownTime = config.Bind<int>(new ConfigDefinition("Questing", "questCooldown"), 65, new ConfigDescription("The cooldown time for a quest to appear (seconds)")).Value;

                // Director params
                worldSpawnPercentage = config.Bind<float>(new ConfigDefinition("Director", "worldSpawnPercentage"), 100.0f, new ConfigDescription("World spawn percentage for the director")).Value / 100;
                bannedDirectorSpawns = config.Bind<string>(new ConfigDefinition("Director", "bannedDirectorSpawns"), "Chest,TripleShop,Chance,Equipment,Blood", new ConfigDescription("A comma seperated list of banned spawns for the director")).Value.Split(',');
                defaultWorldSpawnsEnabled = config.Bind<bool>(new ConfigDefinition("Director", "defaultWorldSpawnsEnabled"), true, new ConfigDescription("Whether or not to use default world spawns or banned spawns list")).Value;

                // Feature params
                questingEnabled = config.Bind<bool>(new ConfigDefinition("Features", "questingEnabled"), true, new ConfigDescription("Quests enabled")).Value;
                enemyItemDropsEnabled = config.Bind<bool>(new ConfigDefinition("Features", "enemyItemDropsEnabled"), true, new ConfigDescription("Enemies drop items")).Value;
                restartQuestsOnStageChange = config.Bind<bool>(new ConfigDefinition("Features", "restartQuestsOnStageChange"), false, new ConfigDescription("Quests reset on stage change")).Value;

                questPort = config.Bind<short>(new ConfigDefinition("Networking", "questPort"), 1337, new ConfigDescription("The port used for the quest networking")).Value;

                Chat.AddMessage("<color=#13d3dd>RPGMod: </color> Config loaded");
            }
        }
    } // namespace Questing
} // namespace RPGMod