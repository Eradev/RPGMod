using BepInEx.Configuration;

namespace RPGMod
{
    public static class Config
    {
        public struct Questing
        {
            public static int cooldown;
            public static float chanceCommon;
            public static float chanceUncommon;
            public static float chanceLegendary;
            public static float chanceAdjustmentPercent;

            public static int killCommonMin;
            public static int killCommonMax;
            public static int killEliteMin;
            public static int killEliteMax;
            public static int killChampionMin;
            public static int killChampionMax;

            public static int killFlyingMin;
            public static int killFlyingMax;
            public static int killHauntedMin;
            public static int killHauntedMax;
            public static int killPoisonMin;
            public static int killPoisonMax;

            public static int collectGoldMin;
            public static int collectGoldMax;
        }

        public struct Networking
        {
            public static short msgType;
            public static short updateRate;
        }

        public struct UI
        {
            public static bool useHUDScale;
            public static float overrideHUDScale;
            public static float questPositionX;
            public static float questPositionY;
            public static float announcerScaleX;
            public static float announcerPositionY;
        }

        public static void Load(ConfigFile config, bool reload)
        {
            if (reload)
            {
                config.Reload();
            }

            // Questing
            Questing.cooldown = config.Bind
            (
                new ConfigDefinition("Questing", "Quest cooldown"),
                45,
                new ConfigDescription("Time needed to wait after quest completion for a new quest to be generated")
            ).Value;

            Questing.chanceCommon = config.Bind
            (
                new ConfigDefinition("Questing", "Common reward chance"),
                76f,
                new ConfigDescription("The percentage chance that the reward for a quest is of common rarity")
            ).Value / 100;
            Questing.chanceUncommon = config.Bind
            (
                new ConfigDefinition("Questing", "Uncommon reward chance"),
                16f,
                new ConfigDescription("The percentage chance that the reward for a quest is of uncommon rarity")
            ).Value / 100;
            Questing.chanceLegendary = config.Bind
            (
                new ConfigDefinition("Questing", "Legendary reward chance"),
                8f,
                new ConfigDescription("The percentage chance that the reward for a quest is of legendary rarity")
            ).Value / 100;

            Questing.chanceAdjustmentPercent = config.Bind
            (
                new ConfigDefinition("Questing", "Chance Adjustment Percentage"),
                2f,
                new ConfigDescription("Multiplied by the amount of quests completed, the percentage to increase the chance of uncommon and legendary rewards and decrease the chance of common quests")
            ).Value / 100;

            Questing.killCommonMin = config.Bind
            (
                new ConfigDefinition("Questing", "Common kill goal minimum"),
                5,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.killCommonMax = config.Bind
            (
                new ConfigDefinition("Questing", "Common kill goal maximum"),
                9,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.killEliteMin = config.Bind
            (
                new ConfigDefinition("Questing", "Elite kill goal minimum"),
                3,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.killEliteMax = config.Bind
            (
                new ConfigDefinition("Questing", "Elite kill goal maximum"),
                7,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.killChampionMin = config.Bind
            (
                new ConfigDefinition("Questing", "Champion kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.killChampionMax = config.Bind
            (
                new ConfigDefinition("Questing", "Champion kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.killFlyingMin = config.Bind
            (
                new ConfigDefinition("Questing", "Flying kill goal minimum"),
                2,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.killFlyingMax = config.Bind
            (
                new ConfigDefinition("Questing", "Flying kill goal maximum"),
                5,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.killHauntedMin = config.Bind
            (
                new ConfigDefinition("Questing", "Celestine kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.killHauntedMax = config.Bind
            (
                new ConfigDefinition("Questing", "Celestine kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.killPoisonMin = config.Bind
            (
                new ConfigDefinition("Questing", "Malachine kill goal minimum"),
                1,
                new ConfigDescription("The minimum amount for the range of the kill goal")
            ).Value;
            Questing.killPoisonMax = config.Bind
            (
                new ConfigDefinition("Questing", "Poison kill goal maximum"),
                2,
                new ConfigDescription("The maximum amount for the range of the kill goal")
            ).Value;

            Questing.collectGoldMin = config.Bind
            (
                new ConfigDefinition("Questing", "Collect gold goal minimum"),
                12,
                new ConfigDescription("The minimum amount for the range of the gold goal, before difficulty scaling (refer to this as the start of the game cost)")
            ).Value;
            Questing.collectGoldMax = config.Bind
            (
                new ConfigDefinition("Questing", "Collect gold goal maximum"),
                31,
                new ConfigDescription("The maximum amount for the range of the gold goal, before difficulty scaling (refer to this as the start of the game cost)")
            ).Value;

            // Networking
            Networking.msgType = config.Bind<short>
            (
                new ConfigDefinition("Networking", "Message type"),
                1337,
                new ConfigDescription("The starting number used for identification of messages sent between the client and server (both clients and server must be the same) (it is highly unlikely existing mods will conflict with the default value)")
            ).Value;
            Networking.updateRate = config.Bind<short>
            (
                new ConfigDefinition("Networking", "Update rate(ms)"),
                100,
                new ConfigDescription("How frequently clients are synced to the server")
            ).Value;

            // UI
            UI.useHUDScale = config.Bind
            (
                new ConfigDefinition("UI", "Use game HUD scale"),
                true,
                new ConfigDescription("Determines whether or not the ingame HUD Scale setting is used")
            ).Value;
            UI.overrideHUDScale = config.Bind
            (
                new ConfigDefinition("UI", "HUD Scale override"),
                1f,
                new ConfigDescription("If useHUDScale is false, this value is used to determine scale instead")
            ).Value / 100;
            UI.questPositionX = config.Bind
            (
                new ConfigDefinition("UI", "Quest position on screen (x axis)"),
                87f,
                new ConfigDescription("Location of the quest on the screen (x axis), represented by screen width percentage")
            ).Value / 100;
            UI.questPositionY = config.Bind
            (
                new ConfigDefinition("UI", "Quest position on screen (y axis)"),
                70f,
                new ConfigDescription("Location of the quest on the screen (y axis), represented by screen height percentage")
            ).Value / 100;
            UI.announcerScaleX = config.Bind
            (
                new ConfigDefinition("UI", "Scale of announcer (x axis)"),
                0.3f,
                new ConfigDescription("Controls the percentage screen width (x axis) used for the announcer box")
            ).Value;
            UI.announcerPositionY = config.Bind
            (
                new ConfigDefinition("UI", "Announcer position on the screen (y axis)"),
                0.03f,
                new ConfigDescription("Location of the announcer on the screen (y axis), represented by screen height percentage")
            ).Value;
        }
    }
}